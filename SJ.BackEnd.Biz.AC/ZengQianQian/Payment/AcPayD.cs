#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcPayD.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：付款体
* 创 建 者：曾倩倩
* 创建时间：2019/9/26 16:51:23
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 付款体
    /// 1 描述：在应收应付指定的科目对账中，根据应付的凭证分录生成的付款单据
    /// 2 约定：
    ///     1.新增时使用parent存储父凭证分录id，科目固定为z90002.付款单；
    ///     2.修改时使用after标识跳过数据校验
    /// 3 业务逻辑：
    ///     新增前：初始化数据父凭证的单据体、对方科目单据体数据
    ///     新增后：新增对方科目单据体后，更新单据头的借贷合计和对方科目的借方金额
    ///     修改前：
    ///         1.根据状态判断单据数据是否能修改，设置能够修改的属性；
    ///         2.校验存在父凭证分录的付款体数据是否合法；
    ///         3.对方科目无变化时，全面校验；发生变化时，清空所有tcode、数量信息、外币信息，并简单校验数据
    ///     修改后：修改存在父凭证的单据体的实付金额后，更新单据头的借贷合计和对方科目的借方金额
    /// </summary>
    [ClassData("cname", "付款体", "vision", 1)]
    public class AcPayD : BizTableVoucherD
    {
        /// <summary>
        /// 预留字段、子查询定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // 字段定义 **********
            // 启用预留字段
            this.AddField("amountp", this.ApiTask.L("提审金额"), EFieldType.数值, RealField: "d0");

            // 更改显示名称
            ((FieldDefine)ListField["ddesc"]).DisplayName = this.ApiTask.L("摘要");
            ((FieldDefine)ListField["camount"]).DisplayName = this.ApiTask.L("原币");
            ((FieldDefine)ListField["unitname"]).DisplayName = this.ApiTask.L("单位");
            ((FieldDefine)ListField["bsign"]).DisplayName = this.ApiTask.L("备用标志");
            ((FieldDefine)ListField["amountc"]).DisplayName = this.ApiTask.L("实付金额");

            ((FieldDefine)ListField["qty"]).DisplayName = this.ApiTask.L("数量");
            ((FieldDefine)ListField["currency"]).DisplayName = this.ApiTask.L("币种");
            ((FieldDefine)ListField["crate"]).DisplayName = this.ApiTask.L("汇率");
            ((FieldDefine)ListField["reference"]).DisplayName = this.ApiTask.L("业务参考");
            ((FieldDefine)ListField["bdate"]).DisplayName = this.ApiTask.L("业务日期");
            ((FieldDefine)ListField["edate"]).DisplayName = this.ApiTask.L("到期日期");

            ((FieldDefine)ListField["vh"]).RefBiz = "AcPay";

            // 下设科目对账属性
            AcVoucherHelper.AddRecoField(this);

            // 子查询定义 **********
            this.AddSubQuery("acvoucherdsub", "凭证分录sub", "AcVoucherDSub", "id=parent");

            // 删除不需要的字段
            this.ListField.Remove("isori"); // 原始标记
        }

        #region 框架方法重写

        /// <summary>
        /// 静态过滤条件
        /// 1.设置科目为付款科目
        /// </summary>
        public override SData BaseParms => new SData("account", $"[{ConstResourceDB.GetAccount(this.ApiTask, AcConst.PayAccount)}]");

        /// <summary>
        /// 新增数据前
        /// 1、存在父凭证的单据体，校验必填
        /// 2、对方科目单据体，校验长度、日期格式
        /// </summary>
        /// <param name="data">实体数据</param>
        protected override void OnInsertBefore(SData data)
        {
            // 判断逻辑 **********
            // * 存在父凭证的单据体，校验必填（实付金额）
            if (data["cd"].ToString() == AcConst.Credit.ToString())
            {
                this.CheckEmptyOrNull(data, true, "amountc");
            }
            else // * 对方科目单据体，校验长度、日期格式
            {
                this.CheckMaxLength("ddesc", 300, data); // 摘要
                this.CheckMaxLength("reference", 100, data); // 业务参考

                this.CheckFormat("bdate", Regx.Date, data); // 业务日期
                this.CheckFormat("edate", Regx.Date, data); // 到期日期
            }

            base.OnInsertBefore(data);
        }

        /// <summary>
        /// 修改数据前
        /// 1、已审的单据不允许修改：“所选记录已审”
        /// 2、修改存在父凭证的单据体后，修改对方科目单据体的借方金额，不需要做数据校验
        /// 3、修改单据头核算期后修改单据体的核算期，不需要做数据校验
        /// 4、存在父凭证的单据体，只允许修改实付金额，草稿状态下，同时修改提审金额
        /// 5、对方科目单据体，忽略修改金额、科目对账、核算期、科目、核算单位、父凭证，待审状态下，只允许修改摘要
        /// 6、校验数据：
        ///             1、初始化数据；
        ///             2、存在父凭证的单据体，校验是否允许付款；
        ///             3、对方科目单据体，科目无变化时，全面校验；
        ///             4、科目发生变化时，清空所有tcode、数量信息、外币信息，并简单校验
        /// </summary>
        /// <param name="oldData">实体原始数据</param>
        /// <param name="updateData">实体修改的数据</param>
        protected override void OnUpdateBefore(SData oldData, SData updateData)
        {
            updateData["vh"] = $"[{oldData["vh.id"]}]";

            // 判断是否审批流程过程中的状态更新，如果不是就走正常流程，是就直接更新状态信息
            if (!updateData.ContainsKey("isapprove"))
            {
                // 数据准备 **********
                // 变量声明
                string state = oldData["vstate", string.Empty].ToString().Sp_First(); // 状态

                // 业务逻辑 **********
                // * 已审的单据不允许修改，抛出异常
                if (state == AcConst.Trial)
                {
                    throw new Exception(this.ApiTask.LEnd("所选记录已审"));
                }

                // * 修改存在父凭证的单据体后，修改对方科目单据体的借方金额，不需要做数据校验
                if (updateData.ContainsKey("amountd")
                    && updateData.ContainsKey("amountc")
                    && updateData["after", string.Empty].ToString() == "操作单据体金额后修改对方科目单据体")
                {
                    updateData.Remove("after");

                    updateData["cd"] = oldData["cd"];
                    updateData["qty"] = oldData["qty"];
                    updateData["crate"] = oldData["crate"];
                    updateData["price"] = oldData["price"];
                    updateData["vh.id"] = oldData["vh.id"];
                    updateData["camount"] = oldData["camount"];

                    // 计算设置汇率、原币金额、单价、数量、金额
                    var dBiz = this.ApiTask.Biz<BizTable>("AcPay"); // 付款头对象
                    var pay = dBiz.GetItemByParms(new SData("id", oldData["vh.id"]).toParmStr(), "account.isqty,account.iscurrency");
                    SData subject = null;
                    if (!string.IsNullOrEmpty(pay.Item<string>("account.isqty")))
                    {
                        if (!string.IsNullOrEmpty(pay.Item<string>("account.iscurrency")))
                        {
                            subject = new SData("isqty", pay["account.isqty"], "iscurrency", pay["account.iscurrency"]);
                            VoucherHelper.SetExchangeRateAndAmount(updateData, this.ApiTask, false, subject);
                        }
                    }

                    return;
                }

                // * 修改单据头核算期后修改单据体的核算期，不需要做数据校验
                if (updateData.ContainsKey("period")
                    && updateData["after", string.Empty].ToString() == "修改单据头核算期后修改单据体")
                {
                    updateData.Remove("after");
                    return;
                }

                // * 存在父凭证的单据体，只允许修改实付金额，草稿状态下，同时修改提审金额
                if (oldData["cd"].ToString() == AcConst.Credit.ToString())
                {
                    // 是否修改实付金额
                    bool isChangeAmount = updateData.ContainsKey("amountc");
                    object updateAmount = updateData["amountc"];
                    updateData.Clear();

                    if (isChangeAmount)
                    {
                        updateData["id"] = oldData["id"];
                        updateData.Add("amountc", updateAmount);

                        // 单据状态为草稿：修改实付金额时需要同时修改提审金额
                        if (state == AcConst.Draft)
                        {
                            updateData.Add("amountp", updateAmount);
                        }
                    }
                }
                else // * 对方科目单据体，忽略修改金额、科目对账、核算期、科目、核算单位、父凭证、计量单位，待审状态下，只允许修改摘要
                {
                    // 待审状态下，只允许修改摘要
                    if (state == AcConst.UnderTrial)
                    {
                        // 是否修改摘要
                        bool isChangeDesc = updateData.ContainsKey("ddesc");
                        object updateDesc = updateData["ddesc"];

                        updateData.Clear();
                        if (isChangeDesc)
                        {
                            updateData.Add("id", oldData["id"]);
                            updateData.Add("ddesc", updateDesc);
                        }
                    }
                    else
                    {
                        updateData.Remove("amount");
                        updateData.Remove("amountd");
                        updateData.Remove("amountc");
                        updateData.Remove("amountp");
                        updateData.Remove("cd");
                        updateData.Remove(AcConst.RecoTcode);
                        updateData.Remove("period");
                        updateData.Remove("account");
                        updateData.Remove("SubUnit");
                        updateData.Remove("parent");
                        updateData.Remove("unitname");
                    }
                }

                // * 校验数据
                if (updateData.Count > 0)
                {
                    // 参数定义 **********
                    // 初始化数据
                    VoucherHelper.SetUpdateData(oldData, updateData);

                    // 存在父凭证的单据体
                    if (oldData["cd"].ToString() == AcConst.Credit.ToString())
                    {
                        // 判断逻辑 **********
                        // 校验必填
                        this.CheckEmptyOrNull(updateData, false, "amountc");

                        // 业务逻辑 **********
                        // 校验是否允许付款
                        updateData[AcConst.RecoTcode] = oldData[AcConst.RecoTcode];
                        updateData["parent"] = oldData["parent"];
                        VoucherHelper.CheckCanPayReceipt(oldData, updateData, this, true);
                    }
                    else // 对方科目单据体
                    {
                        // 判断逻辑 **********
                        this.CheckMaxLength("ddesc", 300, updateData); // 摘要
                        this.CheckMaxLength("reference", 100, updateData); // 业务参考

                        this.CheckFormat("bdate", Regx.Date, updateData); // 业务日期
                        this.CheckFormat("edate", Regx.Date, updateData); // 到期日期

                        // 业务逻辑 **********
                        // 对方科目单据体，科目无变化时，全面校验；科目发生变化时，清空所有tcode、acode、数量信息、外币信息，简单校验
                        var biz = this.ApiTask.Biz<BizTable>("AcPay");
                        var pay = biz.GetItemByParms(new SData("id", oldData["vh.id"]).toParmStr(), "account.id,account.dcode");
                        if (pay != null)
                        {
                            // 得到前端传入的修改前的对方科目
                            string oldAccount = updateData.Item<string>("oldaccount");
                            if (!string.IsNullOrEmpty(oldAccount))
                            {
                                // 得到科目id
                                oldAccount = oldAccount.Sp_First();
                                var accountBiz = this.ApiTask.Biz<BizTableCode>("acaccount");
                                var old = accountBiz.GetIDAuto(oldAccount);
                                int now = -2;
                                if (!string.IsNullOrEmpty(pay.Item<string>("account.id")))
                                {
                                    int.TryParse(pay["account.id"].ToString(), out now);
                                }

                                // 对方科目单据体，科目无变化时，全面校验对方科目单据体
                                if (old == now)
                                {
                                    SData subject = accountBiz.GetItemByParms(new SData("id", now).toParmStr(), $"id,isenable,isqty,iscurrency,currency.id,{AcVoucherHelper.TcodeStr},{AcVoucherHelper.TcodeInfStr},{AcVoucherHelper.TcodeNofStr}"); // 科目
                                    VoucherHelper.CheckAll(updateData, subject, ListField, this);
                                }
                                else // 科目发生变化时，清空所有tcode、数量信息、外币信息，并简单校验数据
                                {
                                    // 清空所有的tcode（忽略科目对账t29）、外币信息、数量信息
                                    for (int i = 0; i < 30; i++)
                                    {
                                        if ("t" + i != AcConst.RecoTcode)
                                        {
                                            updateData["t" + i] = string.Empty;
                                        }
                                    }

                                    updateData["qty"] = string.Empty;
                                    updateData["price"] = string.Empty;
                                    updateData["crate"] = string.Empty;
                                    updateData["camount"] = string.Empty;
                                    updateData["currency"] = string.Empty;
                                }
                            }
                        }
                    }

                    updateData["rver"] = oldData["rver"];
                }
            }

            base.OnUpdateBefore(oldData, updateData);
        }

        /// <summary>
        /// 修改存在父凭证的单据体的实付金额后
        /// 1、更新单据头的借贷合计和对方科目的借方金额
        /// 2、更新勾对记录的金额
        /// </summary>
        /// <param name="oldData">实体原始数据</param>
        /// <param name="updateData">实体修改数据</param>
        /// <param name="modify">原始数据和修改数据比对，产生差异的数据</param>
        protected override void OnUpdateAfter(SData oldData, SData updateData, SData modify)
        {
            // 判断是否审批流程过程中的状态更新，如果不是就走正常流程，是就直接更新状态信息
            if (!updateData.ContainsKey("isapprove"))
            {
                if (oldData["cd"].ToString() == AcConst.Credit.ToString())
                {
                    if (modify.ContainsKey("amountc"))
                    {
                        // * 更新单据头的借贷合计和对方科目的借方金额
                        ChangeAmount(oldData.Item<string>("vh.id"));

                        // * 更新勾对记录的金额
                        var recodBiz = ApiTask.Biz<BizTable>(nameof(AcRecoD));
                        var recod = recodBiz.GetItemByParms(new SData("recpay", oldData["vh.id"], "parent", oldData["parent"]).toParmStr(), "id");
                        if (recod != null)
                        {
                            recodBiz.Update(new SData("id", recod["id"], "amountc", updateData["amountc"], "recoamount", updateData["amountc"]));
                        }
                    }
                }
            }

            base.OnUpdateAfter(oldData, updateData, modify);
        }

        /// <summary>
        /// 查询数据前：设置查询参数的缺省值，用于命中索引
        /// </summary>
        /// <param name="QParm">查询参数</param>
        protected override void OnGetListBefore(SData QParm)
        {
            // 核算期
            if (!QParm.ContainsKey("period"))
            {
                QParm["period"] = "190001:209901";
            }

            // 状态
            if (!QParm.ContainsKey("vstate"))
            {
                QParm["vstate"] = ":" + AcConst.Trial;
            }

            base.OnGetListBefore(QParm);
        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 修改存在父凭证的单据体的实付金额 后
        /// 1、修改单据头的借贷合计
        /// 2、修改对方科目的借方金额
        /// </summary>
        /// <param name="vh">单据头id</param>
        private void ChangeAmount(string vh)
        {
            if (!string.IsNullOrEmpty(vh))
            {
                // 数据准备 **********
                // 变量声明
                // 聚合查询得到单据头下存在父凭证的单据体的实付金额和
                object amountc = this.GetAggregate(new SData("vh", $"[{vh}]", "cd", AcConst.Credit).toParmStr(), EAggregate.Sum, "amountc");
                decimal sumd = 0;
                if (amountc != null)
                {
                    decimal.TryParse(amountc.ToString(), out sumd);
                }

                // 业务逻辑 **********
                // * 修改单据头的借贷合计
                var dBiz = this.ApiTask.Biz<BizTable>("AcPay");
                dBiz.Update(new SData("id", vh, "sumc", sumd, "sumd", sumd, "after", "操作单据体金额后修改单据头"));

                // * 修改对方科目的借方金额
                var subjectD = this.GetItemByParms(new SData("vh", $"[{vh}]", "cd", AcConst.Debit).toParmStr(), "id");
                if (subjectD != null)
                {
                    this.Update(new SData("id", subjectD["id"], "amountd", sumd, "amountc", 0, "after", "操作单据体金额后修改对方科目单据体"));
                }
            }
        }

        #endregion
    }
}
