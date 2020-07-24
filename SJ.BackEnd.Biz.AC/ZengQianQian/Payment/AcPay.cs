#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcPay.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：付款头
* 创 建 者：曾倩倩
* 创建时间：2019/9/26 16:51:15
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 付款头
    /// 1 描述：在应收应付指定的科目对账中，根据应付的凭证分录生成的付款单据
    /// 2 约定：
    ///     1.新增时使用dList存储选择付款的凭证分录，分录中使用parent存储凭证分录id、amountc存储实付金额；
    ///     2.使用subjectD存储对方科目付款体，使用account存储对方科目
    /// 3 业务逻辑：
    ///     新增前：核算期校验，校验是否允许付款，屏蔽父类方法（避免占用凭证的编号）
    ///     新增后：添加单据体，修改单据头的借贷合计
    ///     删除前：已审的单据不允许删除，删除单据头前先删除单据体的数据
    ///     修改前：根据状态判断单据数据是否能修改，设置能够修改的属性，核算期校验
    ///     修改后：修改单据头的核算期后，同步修改单据体
    ///     查询前：处理业务日期、核算单位等查询条件
    /// </summary>
    [ClassData("cname", "付款头", "ApproveFlow", "z03", "vision", 1)]
    public class AcPay : AcPayReceipt
    {
        /// <summary>
        /// 预留字段、参数、子查询定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // 字段定义 **********
            // 启用预留字段
            this.AddField("subrec", this.ApiTask.L("科目对账"), EFieldType.关联, "AcSubRec", "a9");
            this.AddField("account", this.ApiTask.L("对方科目"), EFieldType.关联, "AcAccount", "a8");

            // 更改显示名称
            ((FieldDefine)ListField["dcode"]).DisplayName = this.ApiTask.L("编号");
            ((FieldDefine)ListField["ddesc"]).DisplayName = this.ApiTask.L("摘要");
            ((FieldDefine)ListField["sumd"]).DisplayName = this.ApiTask.L("实付金额");

            ((FieldDefine)ListField["vtype"]).DisplayName = this.ApiTask.L("凭证类型");
            ((FieldDefine)ListField["period"]).DisplayName = this.ApiTask.L("核算期");

            // 设置对象名和中文名审批流程使用
            this.BizD = "AcPayD";
            this.BizName = "付款";

            // 查询参数定义 **********
            // 根据勾对体Id查找付款单头集合(勾对体Id可以多个)
            this.AddParm("recodIds", "勾对Id");

            // 子查询定义 **********
            // 付款体子查询
            this.AddSubQuery("detail", "付款体", "AcPayD", "vh=id");

            // 删除不需要的字段
            this.ListField.Remove("attach"); // 附件张数
        }

        #region 框架方法重写

        /// <summary>
        /// 自定义查询参数的sql处理
        /// </summary>
        /// <param name="parmKey">查询参数key</param>
        /// <param name="parmStr">查询参数值</param>
        /// <returns></returns>
        protected override string CustomParmSql(string parmKey, string parmStr)
        {
            string sql = base.CustomParmSql(parmKey, parmStr);

            // 根据勾对体Id查找付款单头集合(勾对体Id可以多个)
            if (parmKey == "recodIds")
            {
                sql += $@"
exists (select 1  
from 
voucherd as vd 
where 
{{0}}.id = vd.a6 and 
vd.account =(select id from account where dcode='{AcConst.RecoAccount}') and 
vd.id in ({parmStr}))";
            }

            return sql;
        }

        /// <summary>
        /// 新增数据前
        /// 1、初始化数据
        /// 2、校验数据：必填、长度、唯一性、存在性
        /// 3、核算期校验
        /// 4、校验是否允许付款
        /// </summary>
        /// <param name="data">实体数据</param>
        protected override void OnInsertBefore(SData data)
        {
            // 参数定义 **********
            // * 初始化数据
            VoucherHelper.SetInitData(data, this.ApiTask, nameof(AcPay));

            // 判断逻辑 **********
            // * 校验数据：必填、长度、唯一性、存在性
            // 校验必填（科目对账、凭证类型、核算期、对方科目）
            this.CheckEmptyOrNull(data, true, "subrec", "vtype", "period", "account");

            // 校验长度（摘要）
            this.CheckMaxLength("ddesc", 300, data);

            // 存在性判断（科目对账、凭证类型、对方科目）
            this.CheckRefExist(data, "subrec", "vtype", "account");

            // 业务逻辑 **********
            // * 核算期校验
            int.TryParse(data["period"].ToString(), out int period);
            PeriodHelper.Check(period, this.ApiTask);

            // * 校验是否允许付款
            VoucherHelper.CheckCanPayReceipt(data, this, true, false);
        }

        /// <summary>
        /// 新增数据后
        /// 1、添加存在父凭证的单据体
        /// 2、添加勾对记录单据体
        /// 3、添加对方科目单据体
        /// 4、修改单据头的借贷合计
        /// </summary>
        /// <param name="data">实体数据</param>
        protected override void OnInsertAfter(SData data)
        {
            // 数据准备 **********
            // 变量声明
            decimal sum = 0; // 实付金额合计
            var vh = $"[{data["id"]}]"; // 单据头id
            var dBiz = this.ApiTask.Biz<BizTable>("AcPayD"); // 付款体对象
            var dList = data["dList"] as List<SData>; // 选择付款的凭证分录列表
            var accountBiz = this.ApiTask.Biz<BizTableCode>("acaccount"); // 科目对象
            int accountId = accountBiz.GetIDAuto(AcConst.PayAccount); // 付款科目
            var subjectId = data.Item<string>("account.id");

            // 对方科目付款体
            if (data["subjectD"] == null)
            {
                data["subjectD"] = new SData();
            }

            // 得到凭证分录列表
            var vSubBiz = this.ApiTask.Biz<BizTable>("AcVoucherD"); // 凭证分录对象
            var voucherList = new List<SData>();
            if (data["dids"] is SData ids)
            {
                foreach (var item in ids.Keys)
                {
                    string id = ids.Item<string>(item);
                    if (!string.IsNullOrEmpty(id))
                    {
                        id = id.TrimStart(',');
                        if (!string.IsNullOrEmpty(id))
                        {
                            var itemList = vSubBiz.GetListData(0, 1, new SData("id", id).toParmStr(), "id,qty,ddesc,crate,price,bdate,edate,isori,bsign,camount,reference,currency.id,account.id,account.isqty,cd,account.iscurrency," + AcVoucherHelper.TASDCodeStr);
                            if (itemList != null)
                            {
                                if (itemList.Count > 0)
                                {
                                    voucherList.AddRange(itemList);
                                }
                            }
                        }
                    }
                }
            }

            // 业务逻辑 **********
            // * 添加存在父凭证的单据体
            var tickPairDetailList = new List<SData>();
            SData firstTick = null;
            foreach (var item in dList)
            {
                item["vh"] = vh;
                item["cd"] = AcConst.Credit;
                item["amountd"] = 0; // 借方金额为0
                item["account"] = $"[{accountId}]";
                var voucher = voucherList.Find(a => a["id"].ToString() == item["parent"].ToString());
                SData account = new SData("isqty", voucher["account.isqty"], "iscurrency", voucher["account.iscurrency"]);
                VoucherHelper.SetInitData(item, data, voucher, account, this.ApiTask, false);
                int itemId = dBiz.Insert(item);

                decimal.TryParse(item.Item<string>("amountc"), out decimal amountc);
                sum += amountc;

                // 准备添加的勾对记录列表（勾对金额、科目对账、借贷、逻辑标志、父id、状态、子状态、数据类型、核算单位、付款体id、核算期）
                var tick = new SData("recoamount", amountc, AcConst.RecoTcode, $"[{data["subrec.id"]}]", "cd", voucher["cd"], "logicsign", AcConst.RecoVoucher, "parent", voucher["id"], "vstate", $"[{data["vstate.id"]}]", "vsubstate", $"[{data["vsubstate.id"]}]", "vclass", $"[{data["vclass.id"]}]", "SubUnit", $"[{data["SubUnit.id"]}]", "recpay", data["id"], "period", data["period"]);
                if (firstTick == null)
                {
                    firstTick = tick;
                }
                else
                {
                    tickPairDetailList.Add(tick);
                }
            }

            // * 添加勾对记录单据体
            firstTick["recod"] = tickPairDetailList;
            ApiTask.Biz<BizTable>(nameof(AcRecoD)).Insert(firstTick);

            // * 添加对方科目单据体
            var subjectD = data["subjectD"] as SData;
            subjectD["vh"] = vh;
            subjectD["cd"] = AcConst.Debit;
            subjectD["parent"] = string.Empty;
            subjectD["amountc"] = 0; // 贷方金额为0
            subjectD["amountd"] = sum; // 借方金额为实付金额和
            subjectD["account"] = $"[{accountId}]";
            var subject = accountBiz.GetItemByParms(new SData("id", subjectId).toParmStr(), "isqty,iscurrency");
            VoucherHelper.SetInitData(subjectD, data, null, subject, this.ApiTask, true);
            dBiz.Insert(subjectD);

            // * 修改单据头的借贷合计
            this.Update(new SData("id", data["id"], "sumc", sum, "sumd", sum, "after", "操作单据体金额后修改单据头"));

            // 复核父凭证
            var idList = dList.Select(x => x.Item<int>("parent")).ToList();
            AcVoucherHelper.UpdateVSubstateLock(this, idList);

            base.OnInsertAfter(data);
        }

        /// <summary>
        /// 删除数据前
        /// 1、已审的单据不允许删除：“所选记录已审，不允许删除”
        /// 2、删除单据头前先删除勾对记录
        /// 3、删除单据头前先删除单据体的数据
        /// </summary>
        /// <param name="data">实体数据</param>
        protected override void OnDeleteBefore(SData data)
        {
            // * 已审的单据不允许删除，抛出异常
            if (data["vstate", string.Empty].ToString().Sp_First() == AcConst.Trial)
            {
                throw new Exception(this.ApiTask.LEnd("所选记录已审，不允许删除"));
            }

            // * 待审的单据不允许删除，抛出异常
            if (data["vstate", string.Empty].ToString().Sp_First() == AcConst.UnderTrial)
            {
                throw new Exception(this.ApiTask.LEnd("所选记录待审，不允许删除"));
            }

            // * 删除单据头前先删除勾对记录
            string sql = $"delete from voucherd where a6 ={data["id"]}";
            this.ApiTask.DB.ExecuteNonQuery(sql);

            // * 删除单据头前先删除单据体的数据
            var dBiz = ApiTask.Biz<BizTable>("AcPayD");
            payDList = dBiz.GetListData(0, 1, new SData("vh", $"[{data["id"]}]").toParmStr(), "id,parent");
            var dataValidDate = dBiz.DeleteBatch(new SData("vh", $"[{data["id"]}]").toParmStr());

            base.OnDeleteBefore(data);
        }

        /// <summary>
        /// 付款单体列表
        /// </summary>
        List<SData> payDList = new List<SData>();

        /// <summary>
        /// 删除后
        /// 反复核父凭证
        /// </summary>
        /// <param name="Data">实体数据</param>
        protected override void OnDeleteAfter(SData Data)
        {
            // * 反复核父凭证
            var dBiz = ApiTask.Biz<BizTable>("AcPayD");
            var payDIdList = payDList.Select(x => x.Item<int>("id")).ToList();
            var vdIdList = payDList.Select(x => x.Item<int>("parent")).ToList();
            AcVoucherHelper.UpdateVSubstateNoLock(this, vdIdList, payDIdList);

            base.OnDeleteAfter(Data);
        }

        /// <summary>
        /// 修改数据前
        /// 1、已审的单据不允许修改：“所选记录已审”
        /// 2、修改实付金额后，修改单据头的借贷合计，忽略校验
        /// 3、设置草稿的单据允许修改属性值：凭证类型、核算期、摘要、对方科目
        /// 4、设置待审的单据允许修改属性值：摘要
        /// 5、校验数据：必填、长度、存在性、核算期
        /// </summary>
        /// <param name="oldData">实体原始数据</param>
        /// <param name="updateData">实体修改的数据</param>
        protected override void OnUpdateBefore(SData oldData, SData updateData)
        {
            // 判断是否审批流程过程中的状态更新，如果不是就走正常流程，是就直接更新状态信息
            if (!updateData.ContainsKey("isapprove"))
            {
                // 数据准备 **********
                // 变量声明
                string vstate = oldData["vstate", string.Empty].ToString().Sp_First(); // 状态

                // 业务逻辑 **********
                // * 已审的单据不允许修改，抛出异常
                if (vstate == AcConst.Trial)
                {
                    throw new Exception(this.ApiTask.LEnd("所选记录已审"));
                }

                // * 修改实付金额后，修改单据头的借贷合计，忽略校验
                if (updateData.ContainsKey("sumc")
                    && updateData.ContainsKey("sumd")
                    && updateData["after", string.Empty].ToString() == "操作单据体金额后修改单据头")
                {
                    // 实付金额校验（不能为空，不能为0，是数值）
                    AcVoucherHelper.CheckDecimal(this.ApiTask, updateData.Item<string>("sumd"), ((FieldDefine)ListField["sumd"]).DisplayName);

                    updateData.Remove("after");
                    return;
                }

                // * 设置草稿的单据允许修改属性值：凭证类型、核算期、摘要、对方科目
                if (vstate == AcConst.Draft)
                {
                    // 是否修改凭证类型
                    bool isChangeType = updateData.ContainsKey("vtype");
                    object updateType = updateData["vtype"];

                    // 是否修改核算期
                    bool isChangePeriod = updateData.ContainsKey("period");
                    object updatePeriod = updateData["period"];

                    // 是否修改摘要
                    bool isChangeDesc = updateData.ContainsKey("ddesc");
                    object updateDesc = updateData["ddesc"];

                    // 是否修改对方科目
                    bool isChangeAccount = updateData.ContainsKey("account");
                    object updateAccount = updateData["account"];

                    updateData.Clear();
                    if (isChangeType)
                    {
                        updateData.Add("vtype", updateType);
                    }

                    if (isChangePeriod)
                    {
                        updateData.Add("period", updatePeriod);
                    }

                    if (isChangeDesc)
                    {
                        updateData.Add("ddesc", updateDesc);
                    }

                    if (isChangeAccount)
                    {
                        updateData.Add("account", updateAccount);
                    }

                    if (updateData.Count > 0)
                    {
                        updateData.Add("id", oldData["id"]);
                    }
                }
                else if (vstate == AcConst.UnderTrial) // * 设置待审的单据允许修改属性值：摘要
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

                // 判断逻辑 **********
                if (updateData.Count > 0)
                {
                    // * 校验数据：必填、长度、存在性、核算期
                    // 校验必填（科目对账、凭证类型、核算期、对方科目）
                    this.CheckEmptyOrNull(updateData, false, "subrec", "vtype", "period", "account");

                    // 校验长度（摘要）
                    this.CheckMaxLength("ddesc", 300, updateData);

                    // 校验存在性（科目对账、凭证类型、对方科目）
                    this.CheckRefExist(updateData, "subrec", "vtype", "account");

                    // 核算期校验
                    if (updateData.ContainsKey("period"))
                    {
                        int.TryParse(updateData["period"].ToString(), out int period);
                        PeriodHelper.Check(period, this.ApiTask);
                    }

                    updateData["rver"] = oldData["rver"];
                }
            }

            base.OnUpdateBefore(oldData, updateData);
        }

        /// <summary>
        /// 修改数据后：修改单据头的核算期后，修改单据体的核算期
        /// </summary>
        /// <param name="oldData">实体原始数据</param>
        /// <param name="updateData">实体修改数据</param>
        /// <param name="modify">原始数据和修改数据比对，产生差异的数据</param>
        protected override void OnUpdateAfter(SData oldData, SData updateData, SData modify)
        {
            // * 修改单据头的核算期后，修改单据体的核算期
            if (modify.ContainsKey("period"))
            {
                // 数据准备 **********
                // 变量声明
                var period = updateData["period"]; // 核算期
                var dBiz = this.ApiTask.Biz<BizTableVoucherD>("AcPayD"); // 付款体对象
                var dataList = dBiz.GetListData(0, 1, new SData("vh", $"[{oldData["id"]}]").toParmStr(), "id"); // 单据体列表

                // 修改单据体的核算期
                foreach (var item in dataList)
                {
                    dBiz.Update(new SData("id", item["id"], "period", period, "after", "修改单据头核算期后修改单据体"));
                }
            }

            base.OnUpdateAfter(oldData, updateData, modify);
        }

        /// <summary>
        /// 查询数据前
        /// 1、处理区间条件，比如 业务日期开始、业务日期结束 转换为 业务日期
        /// 2、处理核算单位
        /// </summary>
        /// <param name="qParm">查询参数</param>
        protected override void OnGetListBefore(SData qParm)
        {
            // * 处理区间条件 b:e
            qParm.ConvertDate("bdate");
            qParm.ConvertDate("mdate");
            qParm.ConvertDate("period");

            // * 处理核算单位
            this.ApiTask.SubUnitProcess(qParm);

            base.OnGetListBefore(qParm);
        }

        #endregion
    }
}
