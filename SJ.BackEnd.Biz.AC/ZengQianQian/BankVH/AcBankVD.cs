#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcBankVD.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：银行对账单
* 创 建 者：曾倩倩
* 创建时间：2020/4/30 10:10:29
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 银行对账单
    /// 1 描述：无
    /// 2 约定：无
    /// 3 业务逻辑：
    ///     新增前：金额校验，已设置银行对账，核算期赋值，业务参考唯一性，自动生成银行对账单科目，计算金额、借贷，添加银行对账单头
    ///     删除前：核算期小于当前核算期不能删除，更新凭证分录的勾对金额、勾对状态
    ///     删除后：删除银行对账单头
    ///     查询前：处理业务日期、核算单位等查询条件，排序（核算期倒序、业务日期升序、id升序）
    /// </summary>
    [ClassData("cname", "银行对账单", "vision", 1)]
    public class AcBankVD : AcVoucherD
    {
        /// <summary>
        /// 预留字段、子查询定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // 字段定义 **********
            // 启用预留字段
            this.AddField("rstate", ApiTask.L("勾对状态"), EFieldType.关联, "AcRecoState", "a9"); // 勾对状态：从未勾对、部分勾对和全部勾对
            this.AddField("logicsign", ApiTask.L("勾对类型"), EFieldType.关联, "ACRecoType", "a8");
            this.AddField("recoperson", ApiTask.L("勾对人"), EFieldType.关联, "AcUser", "a7");
            this.AddField("bkaccount", this.ApiTask.L("银行科目"), EFieldType.关联, "AcAccount", "a6"); // 不带z的
            this.AddField("bank", this.ApiTask.L("银行账户"), EFieldType.关联, "AcBank", "a5");
            this.AddField("bparent", ApiTask.L("银行父Id"), EFieldType.整数, string.Empty, "a4");
            this.AddField("isinit", ApiTask.L("期初否"), EFieldType.开关, string.Empty, "a3");

            this.AddField("recoamount", ApiTask.L("勾对金额"), EFieldType.数值, string.Empty, "d0");
            this.AddField("recodate", ApiTask.L("勾对日期"), EFieldType.日期, "AcUser", "s0");

            // 字段定义 **********
            // 更改显示名称
            ((FieldDefine)ListField["ddesc"]).DisplayName = this.ApiTask.L("摘要");
            ((FieldDefine)ListField["amount"]).DisplayName = this.ApiTask.L("金额");
            ((FieldDefine)ListField["period"]).DisplayName = this.ApiTask.L("核算期");
            ((FieldDefine)ListField["bdate"]).DisplayName = this.ApiTask.L("业务日期");
            ((FieldDefine)ListField["amountd"]).DisplayName = this.ApiTask.L("借方金额");
            ((FieldDefine)ListField["amountc"]).DisplayName = this.ApiTask.L("贷方金额");
            ((FieldDefine)ListField["subunit"]).DisplayName = this.ApiTask.L("核算单位");
            ((FieldDefine)ListField["reference"]).DisplayName = this.ApiTask.L("业务参考");
            ((FieldDefine)ListField["account"]).DisplayName = this.ApiTask.L("银行对账单科目");

            // 更改单据头对象
            ((FieldDefine)ListField["vh"]).RefBiz = "AcBankVh";

            // 删除不需要的字段
            this.ListField.Remove("isori"); // 原始标记
        }

        #region 框架方法重写

        /// <summary>
        /// 静态过滤条件
        /// 设置头的类型为对账单
        /// </summary>
        public override SData BaseParms => new SData("vh.vtype", AcConst.BankVhType);

        /// <summary>
        /// 新增数据前（使用ignorecheck不走校验逻辑2-6）
        /// 1、初始化数据
        /// 2、校验数据：必填、长度、存在性、格式
        /// 3、金额校验（vision导入金额amount时，设置借方金额、贷方金额）、计算金额、借贷
        /// 4、已设置银行对账
        /// 5、唯一性：业务参考（同一个核算单位、银行科目、银行账户中）
        /// 6、自动生成银行对账单科目
        /// 7、添加银行对账单头
        /// </summary>
        /// <param name="data">实体数据</param>
        protected override void OnInsertBefore(SData data)
        {
            if (data != null)
            {
                // * 初始化数据
                this.InitData(data);

                if (!data.ContainsKey("ignorecheck"))
                {
                    // 判断逻辑 **********
                    // * 校验数据：必填、长度、存在性、格式
                    // 校验必填（核算单位、银行科目、银行账户、业务参考）
                    this.CheckEmptyOrNull(data, true, "subunit", "bkaccount", "bank", "reference");

                    // 校验长度（摘要、业务参考）
                    this.CheckMaxLength("ddesc", 300, data);
                    this.CheckMaxLength("reference", 100, data);

                    // 存在性判断（核算单位、银行科目、银行账户）
                    this.CheckRefExist(data, "subunit", "bkaccount", "bank");

                    // 转换业务日期
                    if (DateTime.TryParse(data.Item<string>("bdate"), out var bDate))
                    {
                        data["bdate"] = bDate.ToStr();
                    }
                    else
                    {
                        if (DateTime.TryParseExact(data.Item<string>("bdate"), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out bDate))
                        {
                            data["bdate"] = bDate.ToStr();
                        }
                    }

                    // 格式判断
                    this.CheckFormat("bdate", Regx.Date, data); // 业务日期

                    // * 根据核算单位、银行科目、银行账户，查询【银行对账】无数据，提示：未设置银行对账，不允许新增。
                    var bankABiz = this.ApiTask.Biz<BizTableCode>("AcBankA"); // 银行对账对象
                    var bankA = bankABiz.GetItemByParms(new SData("subunit", data["subunit"], "bkaccount", data["bkaccount"], "bank", data["bank"]).toParmStr(), "id,isinitbk");
                    if (bankA == null)
                    {
                        throw new Exception(this.ApiTask.LEnd("未设置银行对账，不允许新增"));
                    }

                    // * 核算期：期初->190001，非期初->当前核算期
                    data["period"] = data.Item<bool>("isinit") ? AcConst.PeriodBegin : PeriodHelper.GetCurrent(this.ApiTask).ToString();

                    // 业务逻辑 **********
                    // * 处理金额：金额校验，金额反向
                    HandleAmount(data);

                    // * 同一个核算单位、银行科目、银行账户中，业务参考重复，提示：业务参考重复
                    if (!this.IsUnique(new SData("subunit", data["subunit"], "bkaccount", data["bkaccount"], "bank", data["bank"], "reference", data["reference"])))
                    {
                        throw new Exception(((FieldDefine)ListField["reference"]).DisplayName + this.ApiTask.LAma(AcLang.Repeat));
                    }
                }

                // * 计算金额、借贷
                decimal.TryParse(data["amountd", "0"]?.ToString(), out decimal debitAmount); // 借方金额
                decimal.TryParse(data["amountc", "0"]?.ToString(), out decimal creditAmount); // 贷方金额
                data["amount"] = debitAmount - creditAmount; // 金额：借-贷
                data["cd"] = debitAmount != 0 ? AcConst.Debit : AcConst.Credit;

                // * 科目：自动生成银行对账单科目
                var account = VoucherHelper.AddBankAccount(this.ApiTask, data["bkaccount"]);
                data["account"] = account["dcode"];

                // * 添加对账单头
                var vhData = new SData("period", data["period"], "subunit", data["subunit"], "bdate", data["bdate"], "ddesc", data["ddesc"], "sumd", data["amountd"], "sumc", data["amountc"]); // 得到对账单头对象
                var vhBiz = this.ApiTask.Biz<BizTable>("AcBankVh"); // 得到对账单头访问api
                data["vh"] = $"[{vhBiz.Insert(vhData)}]"; // 得到添加的对账单头id
            }
        }

        /// <summary>
        /// 屏蔽基类
        /// </summary>
        /// <param name="data">实体数据</param>
        protected override void OnInsertAfter(SData data)
        {
        }

        /// <summary>
        /// 删除前
        /// 1、已勾对的银行对账单不允许删除
        /// </summary>
        /// <param name="data">实体数据</param>
        protected override void OnDeleteBefore(SData data)
        {
            // * 删除前，已勾对的银行对账单不允许删除
            if (!string.IsNullOrEmpty(data.Item<string>("rstate")))
            {
                if (data["rstate"].ToString().Sp_First() != AcConst.NeverTick)
                {
                    throw new Exception(this.ApiTask.LEnd("所选记录已勾对，不允许删除"));
                }
            }
        }

        /// <summary>
        /// 删除后
        /// 1、删除对账单头
        /// </summary>
        /// <param name="data">实体数据</param>
        protected override void OnDeleteAfter(SData data)
        {
            // * 删除对账单头
            if (!string.IsNullOrEmpty(data.Item<string>("vh.id")))
            {
                var vhBiz = this.ApiTask.Biz<BizTable>("AcBankVh"); // 得到对账单头访问api
                vhBiz.DeleteByID(int.Parse(data["vh.id"].ToString()));
            }
        }

        /// <summary>
        /// 屏蔽基类
        /// </summary>
        /// <param name="oldData">实体原始数据</param>
        /// <param name="updateData">实体修改数据</param>
        protected override void OnUpdateBefore(SData oldData, SData updateData)
        {
        }

        /// <summary>
        /// 屏蔽基类
        /// </summary>
        /// <param name="oldData">实体原始数据</param>
        /// <param name="updateData">实体修改数据</param>
        /// <param name="modify">修改属性</param>
        protected override void OnUpdateAfter(SData oldData, SData updateData, SData modify)
        {
        }

        /// <summary>
        /// 查询前
        /// 1、处理区间条件，比如 业务日期开始、业务日期结束 转换为 业务日期
        /// 2、处理核算单位
        /// 3、排序：核算期倒序、业务日期升序
        /// </summary>
        /// <param name="qParm">查询参数</param>
        protected override void OnGetListBefore(SData qParm)
        {
            // * 处理区间条件 b:e
            qParm.ConvertDate("bdate"); // 业务日期
            qParm.ConvertDate("period"); // 核算期

            // * 处理核算单位
            this.ApiTask.SubUnitProcess(qParm);

            // * 排序
            if (string.IsNullOrEmpty(qParm.Item<string>("q.orderby")))
            {
                qParm["q.orderby"] = "descperiod,bdate,id";
            }

            base.OnGetListBefore(qParm);
        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="bankSmD">银行对账单体</param>
        private void InitData(SData bankSmD)
        {
            // 业务日期输入为空时，设置业务日期为当前日期
            if (string.IsNullOrEmpty(bankSmD.Item<string>("bdate")))
            {
                bankSmD["bdate"] = DateTime.Now.ToString("yyyy-MM-dd"); // 业务日期
            }

            bankSmD["vstate"] = AcConst.Trial; // 单据状态：已审
            bankSmD["vsubstate"] = AcConst.LockS; // 子状态：锁定
            bankSmD["vclass"] = AcConst.ActData; // 数据类型：实际数据
            bankSmD["rstate"] = bankSmD["rstate", AcConst.NeverTick]; // 勾对状态：从未勾对
            bankSmD["edate"] = DateTime.Now.ToString("yyyy-MM-dd"); // 到期日期：当前日期
        }

        /// <summary>
        /// 处理金额
        /// </summary>
        /// <param name="data">银行对账单体</param>
        private void HandleAmount(SData data)
        {
            // * vision导入金额时，设置借方金额、贷方金额
            decimal amount = 0;
            if (!string.IsNullOrEmpty(data.Item<string>("amount")))
            {
                // 金额校验（可以为0，是数值）
                AcVoucherHelper.CheckDecimal(this.ApiTask, data.Item<string>("amount"), ((FieldDefine)ListField["amount"]).DisplayName, false);
                amount = data.Item<string>("amount").ToDec();
            }

            if (amount > 0) // 金额为正数：金额就在借方
            {
                data["amountd"] = amount;
                data["amountc"] = 0;
            }
            else if (amount < 0) // 金额为负数：金额就在贷方
            {
                data["amountd"] = 0;
                data["amountc"] = -amount;
            }

            // * 借方金额、贷方金额都为空，提示：借贷金额不能都为空。
            var amountd = data.Item<string>("amountd"); // 借方金额
            var amountc = data.Item<string>("amountc"); // 贷方金额
            if (string.IsNullOrEmpty(amountc) && string.IsNullOrEmpty(amountd))
            {
                throw new Exception(this.ApiTask.LEnd("借贷金额不能都为空"));
            }

            // 借方金额、贷方金额为数值
            amountd = string.IsNullOrEmpty(amountd) ? "0" : amountd;
            amountc = string.IsNullOrEmpty(amountc) ? "0" : amountc;
            AcVoucherHelper.CheckDecimal(this.ApiTask, amountd, this.ApiTask.L("借方金额"), false);
            AcVoucherHelper.CheckDecimal(this.ApiTask, amountc, this.ApiTask.L("贷方金额"), false);

            // * 借方金额和贷方金额输入0，当空处理
            decimal amountdD = amountd.ToDec();
            decimal amountcD = amountc.ToDec();
            if (amountdD == 0 && amountcD == 0)
            {
                throw new Exception(this.ApiTask.LEnd("借贷金额不能都为空"));
            }

            // * 借方金额、贷方金额都有值，提示：借贷金额不能都有值。
            if (amountdD != 0 && amountcD != 0)
            {
                throw new Exception(this.ApiTask.LEnd("借贷金额不能都有值"));
            }

            // * 金额反向
            if (amount == 0)
            {
                var temp = data["amountd"];
                data["amountd"] = data["amountc"];
                data["amountc"] = temp;
            }
        }

        #endregion
    }
}
