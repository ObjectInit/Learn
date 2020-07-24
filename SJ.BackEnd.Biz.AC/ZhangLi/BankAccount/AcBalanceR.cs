#region  <<版本注释>>
/* ========================================================== 
// <copyright file="AcBalanceR.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcBalanceR
* 创 建 者：张莉
* 创建时间：2020/7/6 10:10:09
* =============================================================*/
#endregion

using System;
using System.Collections.Generic;
using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 余额调节表
    /// 1 描述：无
    /// 2 约定：
    ///     前端传参key：查询参数（bankaid=银行对账Id）
    ///     返回数据key：银行日记账余额vdbalance、银行对账单余额bkbalance、银收企未收bkvc、企收银未收covd、银付企未付bkvd、企付银未付covc、调节后余额vdafter、bkafter
    ///     返回其他key：核算单位subunit、银行账户bank、银行科目bkaccount、银行科目启用的银行对账tcode
    /// 3 业务逻辑：
    ///     查询后：计算余额调节表数据，并绑定数据传递给前端
    /// </summary>
    [ClassData("cname", "余额调节表")]
    public class AcBalanceR : BizQuery
    {
        /// <summary>
        /// 字段定义、查询参数定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // * 字段定义
            // 数据显示字段
            this.AddField("vdbalance", ApiTask.L("银行日记账余额"), EFieldType.字符串, RealField: "dcode");
            this.AddField("bkbalance", ApiTask.L("银行对账单余额"), EFieldType.字符串, RealField: "dcode");
            this.AddField("bkvc", ApiTask.L("银收企未收"), EFieldType.字符串, RealField: "dcode");
            this.AddField("covd", ApiTask.L("企收银未收"), EFieldType.字符串, RealField: "dcode");
            this.AddField("bkvd", ApiTask.L("银付企未付"), EFieldType.字符串, RealField: "dcode");
            this.AddField("covc", ApiTask.L("企付银未付"), EFieldType.字符串, RealField: "dcode");
            this.AddField("vdafter", ApiTask.L("企业账调节后余额"), EFieldType.字符串, RealField: "dcode");
            this.AddField("bkafter", ApiTask.L("银行账调节后余额"), EFieldType.字符串, RealField: "dcode");

            // 其他显示字段：核算单位subunit、银行账户bank、银行科目bkaccount、银行科目启用的银行对账tcode
            this.AddField("subunit", ApiTask.L("核算单位"), EFieldType.字符串, RealField: "dcode");
            this.AddField("bank", ApiTask.L("银行账户"), EFieldType.字符串, RealField: "dcode");
            this.AddField("bkaccount", ApiTask.L("银行科目"), EFieldType.字符串, RealField: "dcode");
            this.AddField("tcode", ApiTask.L("银行科目启用的银行对账tcode"), EFieldType.字符串, RealField: "dcode");

            // 查询参数
            this.AddParm("bankaid", ApiTask.L("银行对账Id"), EFieldType.整数);
        }

        #region 框架方法重写

        /// <summary>
        /// 初始化sql
        /// </summary>
        /// <param name="sql">查询sql</param>
        protected override void OnInitBaseSql(ref DBSql sql)
        {
            sql = new DBSql
            {
                SqlFrom = new SData("m", "account m"),
                SqlWhere = "id = -1",
                SqlOrderBy = "id",
            };
        }

        /// <summary>
        /// 初始定义未做业务,子类必须实现
        /// </summary>
        protected override void OnInitDefine()
        {
        }

        /// <summary>
        /// 查询后逻辑处理：
        /// 1、获取银行对账实体
        /// 2、获取银行科目对应的银行账户tcode
        /// 3、计算银行日记账余额
        /// 4、计算银行对账单余额
        /// 5、计算企收银未收
        /// 6、计算企付银未付
        /// 7、计算银收企未收
        /// 8、计算银付企未付
        /// 9、计算调节后余额
        /// 10、绑定余额调节表数据传递给前端
        /// </summary>
        /// <param name="qParm">查询条件数据</param>
        /// <param name="data">查询的数据集合</param>
        protected override void OnGetListAfter(SData qParm, List<SData> data)
        {
            // * 获取银行对账实体
            var bankAId = qParm.Item<int>("bankaid");
            var bankA = this.ApiTask.Biz<BizTable>(nameof(AcBankA)).GetItem(bankAId, $"subunit.id,bank.id,bkaccount.id,subunit,bank,bkaccount,bkamount");
            if (bankA == null)
            {
                throw new Exception(ApiTask.LEnd("银行对账不存在"));
            }

            // * 获取银行科目对应的银行账户tcode
            var bkaccountId = bankA.Item<string>("bkaccount.id");
            var tcodeData = AcVoucherHelper.GetBCTcode(bankA.Item<string>("bkaccount.id"), ApiTask);
            var tcode = tcodeData.Item<string>(bkaccountId);

            // * 计算银行日记账余额
            var vdBalance = this.CalVdBalance(bankA, tcode);

            // * 计算银行对账单余额
            var bkBalance = this.CalBkBalance(bankA);

            // * 计算企收银未收
            var coVD = this.GetNoRecoAmountSum(bankA, tcode, nameof(AcCorpVD), "amountd");

            // * 计算企付银未付
            var coVC = this.GetNoRecoAmountSum(bankA, tcode, nameof(AcCorpVD), "amountc");

            // * 计算银收企未收
            var bKVC = this.GetNoRecoAmountSum(bankA, tcode, nameof(AcBankVD), "amountd");

            // * 计算银付企未付
            var bKVD = this.GetNoRecoAmountSum(bankA, tcode, nameof(AcBankVD), "amountc");

            // * 计算调节后余额
            var vdAfter = vdBalance + bKVC - bKVD;
            var bkAfter = bkBalance + coVD - coVC;

            // * 绑定余额调节表数据传递给前端
            data.Add(new SData("subunit", bankA["subunit"], "bank", bankA["bank"], "bkaccount", bankA["bkaccount"], "tcode", tcode, "vdBalance", vdBalance, "bkBalance", bkBalance, "coVD", coVD, "coVC", coVC, "bKVC", bKVC, "bKVD", bKVD, "vdAfter", vdAfter, "bkAfter", bkAfter));

            base.OnGetListAfter(qParm, data);
        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 得到（企业账、银行账）默认的查询条件
        /// </summary>
        /// <returns></returns>
        private SData GetDefaultParm()
        {
            SData queryParms = new SData();
            queryParms["period"] = ":" + AcConst.PeriodEnd; // 最大核算期

            // 隐含条件,实际数据,记账状态,
            var vclassId = ConstResourceDB.GetId(this.ApiTask, nameof(AcVclass), AcConst.ActData);
            queryParms.Append("vclass", $"[{vclassId}]");

            var vstateId = ConstResourceDB.GetId(this.ApiTask, nameof(AcVstate), AcConst.Trial);
            queryParms.Append("vstate", $"[{vstateId}]");

            return queryParms;
        }

        /// <summary>
        /// 计算银行日记账余额
        /// </summary>
        /// <param name="bankA">银行对账</param>
        /// <param name="tcode">银行账户tcode key</param>
        /// <returns></returns>
        private decimal CalVdBalance(SData bankA, string tcode)
        {
            // 查询条件
            var queryParms = GetDefaultParm();
            queryParms.Append("subunit", $"[{bankA["subunit.id"]}]");
            queryParms.Append("account", $"[{bankA["bkaccount.id"]}]"); // 科目
            queryParms["period"] = AcConst.PeriodEnd; // 核算期
            queryParms["vh.vtype"] = $"!{AcConst.BankVhType}"; // 凭证分录

            // 如果银行科目开启了Tcode 需要传入tcode 条件
            if (!string.IsNullOrEmpty(tcode))
            {
                queryParms.Append(tcode, $"[{bankA["bank.id"]}]"); // 银行账户tcode
            }

            // 调用余额表实体，查询企业账期末余额
            var vdBalance = ApiTask.GetBalance(EBalanceType.l, "amount", queryParms.toParmStr())?.ToString().ToDec() ?? 0;
            return vdBalance;
        }

        /// <summary>
        /// 计算银行对账单余额
        /// </summary>
        /// <param name="bankA">银行对账</param>
        /// <returns></returns>
        private decimal CalBkBalance(SData bankA)
        {
            // 查询条件
            var queryParms = GetDefaultParm();
            queryParms.Append("subunit", $"[{bankA["subunit.id"]}]");
            queryParms.Append("bkaccount", $"[{bankA["bkaccount.id"]}]"); // 银行科目
            queryParms.Append("bank", $"[{bankA["bank.id"]}]"); // 银行账户
            queryParms["vh.vtype"] = AcConst.BankVhType; // 银行对账单
            queryParms["period"] = $"!{AcConst.PeriodBegin}"; // 核算期大于190001的对账单余额

            var bkamount = bankA.Item<decimal>("bkamount"); // 对账单期初余额

            // 聚合获取银行对账单余额
            var biz = ApiTask.Biz<BizTable>(nameof(AcBankVD));
            var bkBalance = biz.GetAggregate(queryParms.toParmStr(), EAggregate.Sum, "amount")?.ToString().ToDec() ?? 0;
            return bkBalance + bkamount;
        }

        /// <summary>
        /// 根据金额类型得到金额之和、已勾对金额之和、未勾对金额之和
        /// </summary>
        /// <param name="bankA">银行对账</param>
        /// <param name="tcode">银行账户tcode key</param>
        /// <param name="bizStr">实体名</param>
        /// <param name="amountFiled">聚合字段</param>
        /// <returns>未勾对金额之和</returns>
        private decimal GetNoRecoAmountSum(SData bankA, string tcode, string bizStr, string amountFiled)
        {
            // 根据金额类型得到查询条件
            var queryParms = GetDefaultParm();
            queryParms.Append("subunit", $"[{bankA["subunit.id"]}]");

            if (bizStr == nameof(AcCorpVD))
            {
                queryParms.Append("account", $"[{bankA["bkaccount.id"]}]"); // 科目
                if (!string.IsNullOrEmpty(tcode))
                {
                    queryParms.Append(tcode, $"[{bankA["bank.id"]}]"); // 银行账户tcode
                }

                queryParms["recostate"] = AcConst.NoAllTick; // 勾对状态：未勾对
                queryParms["vh.vtype"] = $"!{AcConst.BankVhType}"; // 凭证分录
            }
            else if (bizStr == nameof(AcBankVD))
            {
                queryParms.Append("bkaccount", $"[{bankA["bkaccount.id"]}]"); // 银行科目
                queryParms.Append("bank", $"[{bankA["bank.id"]}]"); // 银行账户
                queryParms["rstate"] = $"!{AcConst.AllTick}"; // 勾对状态：未勾对
                queryParms["vh.vtype"] = AcConst.BankVhType; // 银行对账单
            }

            queryParms[amountFiled] = "!0"; // 金额在借方/贷方

            // 金额字段：借方金额、贷方金额
            var biz = ApiTask.Biz<BizTable>(bizStr);

            // 得到金额之和
            var amountSum = biz.GetAggregate(queryParms.toParmStr(), EAggregate.Sum, amountFiled)?.ToString().ToDec() ?? 0;

            // 得到已勾对金额之和
            var recoSum = biz.GetAggregate(queryParms.toParmStr(), EAggregate.Sum, "recoamount")?.ToString().ToDec() ?? 0;

            // 未勾对金额之和 = 金额之和 - 已勾对金额之和
            var noRecoSum = amountSum - recoSum;
            return noRecoSum;
        }
        #endregion
    }
}
