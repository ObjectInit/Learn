#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcRecPayQuery.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcRecPayQuery
* 创 建 者：曾倩倩
* 创建时间：2020/7/3 9:41:33
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.Global;
using System;
using System.Collections.Generic;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 勾对余额
    /// 1 描述：无
    /// 2 约定：
    ///     1、查询时使用calsum，后端将返回借方金额合计sumd、贷方金额合计sumc、借方已勾对金额合计sumdreco、借方未勾对金额合计sumdunreco、贷方已勾对金额合计sumcreco、贷方未勾对金额合计sumcunreco、余额合计sumend、应收余额合计sumrecend、应付余额合计sumpayend、总条数datacount、总页数pagecount
    /// 3 业务逻辑：
    ///     查询后：处理每行数据的代码、名称、金额，计算合计行的金额，返回总条数、总页数
    /// </summary>
    [ClassData("cname", "勾对余额")]
    public class AcRecPayQuery : BizQuery
    {
        /// <summary>
        /// 预留字段、查询参数定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // 字段定义 **********
            // 启用预留字段
            this.AddField("dcode", "代码", EFieldType.字符串, RealField: "dcode");
            this.AddField("title", "名称", EFieldType.字符串, RealField: "dcode");
            this.AddField("amountd", "借方金额", EFieldType.字符串, RealField: "dcode");
            this.AddField("amountc", "贷方金额", EFieldType.字符串, RealField: "dcode");
            this.AddField("amountdreco", "借方已勾对金额", EFieldType.字符串, RealField: "dcode");
            this.AddField("amountdunreco", "借方未勾对金额", EFieldType.字符串, RealField: "dcode");
            this.AddField("amountcreco", "贷方已勾对金额", EFieldType.字符串, RealField: "dcode");
            this.AddField("amountcunreco", "贷方未勾对金额", EFieldType.字符串, RealField: "dcode");
            this.AddField("amountend", "余额", EFieldType.字符串, RealField: "dcode");
            this.AddField("amountrecend", "应收余额", EFieldType.字符串, RealField: "dcode");
            this.AddField("amountpayend", "应付余额", EFieldType.字符串, RealField: "dcode");

            // 查询参数定义 **********
            this.AddParm("groupkey", "分组key", EFieldType.字符串);
            this.AddParm("pageindex", "第xx页", EFieldType.字符串);
            this.AddParm("pagesize", "一页xx条", EFieldType.字符串);
            this.AddParm("subrec", "科目对账", EFieldType.字符串);
            this.AddParm("billtype", "单据类型", EFieldType.字符串);
            this.AddParm("account", "科目", EFieldType.字符串);
            this.AddParm("currency", "币种", EFieldType.字符串);

            // tcode
            var data = ApiTask.Tcode();
            foreach (var item in data)
            {
                if (!item.Key.Contains(AcConst.RecoTcode))
                {
                    this.AddParm(item.Key, item.Value.ToString(), EFieldType.字符串);
                }
            }
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
        /// 查询后
        /// 1、计算每行数据的已勾对金额（借方）、未勾对金额（借方）、已勾对金额（贷方）、未勾对金额（贷方）、余额
        /// 2、赋值合计行的金额
        /// 3、赋值总条数、总页数
        /// </summary>
        /// <param name="qParm">查询参数</param>
        /// <param name="data">数据集合</param>
        protected override void OnGetListAfter(SData qParm, List<SData> data)
        {
            // 处理前端传入的查询参数
            HandleQueryParams(qParm, out int size, out int index, out string groupkey);

            if (!string.IsNullOrEmpty(groupkey))
            {
                // 查询余额表数据
                string filed = groupkey + "," + "sb.a_sum_amount,sh.a_sum_amountd,sh.a_sum_amountc";
                var datas = ApiTask.Biz<BizQuery>(nameof(AcBalance)).GetList(size, index, qParm.toParmStr(), filed);
                var datalist = datas["data"] as List<SData>;
                var pager = datas["pager"] as SData;

                // * 赋值每行数据的代码、名称、金额
                HandleDataRow(datalist, groupkey, qParm);

                if (qParm.ContainsKey("calsum"))
                {
                    // * 赋值合计行的金额
                    HandleSum(datalist, groupkey, qParm);

                    // * 赋值总条数、总页数
                    if (datalist.Count > 0)
                    {
                        datalist[0]["datacount"] = pager["rowCount"]; // 总条数
                        datalist[0]["pagecount"] = pager["PageCount"]; // 总页数
                    }
                }

                data.AddRange(datalist);
            }
        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 得到查询条件
        /// </summary>
        /// <param name="queryParams">查询条件</param>
        /// <param name="acUnitKey">往来单位tcode key</param>
        /// <param name="unit">数据行的往来单位值</param>
        /// <returns></returns>
        private SData GetQueryVoucherDSub(SData queryParams, string acUnitKey, string unit)
        {
            // 得到查询条件
            var newQueryAcVoucherD = new SData(AcConst.RecoTcode, queryParams["subrec"], "vstate", $"{AcConst.Trial}", "account", AcConst.RecoAccount);
            foreach (var key in queryParams.Keys)
            {
                newQueryAcVoucherD[$"AcVoucherDSub.{key}"] = queryParams[key];
            }

            newQueryAcVoucherD[$"AcVoucherDSub.{acUnitKey}"] = unit; // 往来单位取数据行的值

            return newQueryAcVoucherD;
        }

        /// <summary>
        /// 得到勾对金额
        /// </summary>
        /// <param name="parm">查询条件</param>
        /// <returns></returns>
        private decimal GetRecoAmount(SData parm)
        {
            var amount = this.ApiTask.Biz<BizTable>("AcRecoD").GetAggregate(
                                         parm.toParmStr(),
                                         EAggregate.Sum,
                                         Field: "recoamount") ?? 0;
            return Convert.ToDecimal(amount);
        }

        /// <summary>
        /// 处理查询条件：一页xx条数据、第xx页、往来单位、科目对账
        /// </summary>
        /// <param name="qparm">查询条件</param>
        /// <param name="size">一页xx条数据</param>
        /// <param name="index">第xx页</param>
        /// <param name="unitkey">往来单位tcode key</param>
        private void HandleQueryParams(SData qparm, out int size, out int index, out string unitkey)
        {
            // 科目对账不能为空
            if (string.IsNullOrEmpty(qparm.Item<string>("subrec")))
            {
                throw new Exception(this.ApiTask.L("科目对账") + this.ApiTask.L(AcLang.NotEmpty) + this.ApiTask.L(AcLang.SymbolEnd));
            }

            this.ApiTask.SubUnitProcess(qparm);

            // 一页xx条数据
            size = 10;
            if (!string.IsNullOrEmpty(qparm.Item<string>("pagesize")))
            {
                int.TryParse(qparm["pagesize"].ToString(), out size);
                qparm.Remove("pagesize");
            }

            // 第xx页
            index = 1;
            if (!string.IsNullOrEmpty(qparm.Item<string>("pageindex")))
            {
                int.TryParse(qparm["pageindex"].ToString(), out index);
                qparm.Remove("pageindex");
            }

            // 往来单位tcode key
            unitkey = string.Empty;
            if (!string.IsNullOrEmpty(qparm.Item<string>("groupkey")))
            {
                unitkey = qparm["groupkey"].ToString();
                qparm.Remove("groupkey");
            }

            // 科目对账
            qparm["account.id"] = VoucherHelper.GetAccountBySubrec(this.ApiTask, qparm.Item<string>("subrec"), qparm.Item<string>("billtype"), qparm.Item<string>("account"));
            qparm.Remove("account");
            qparm.Remove("billtype");
        }

        /// <summary>
        /// 赋值每行数据的代码、名称、金额
        /// </summary>
        /// <param name="datalist">列表数据</param>
        /// <param name="groupkey">分组key</param>
        /// <param name="qParm">查询条件</param>
        private void HandleDataRow(List<SData> datalist, string groupkey, SData qParm)
        {
            var newQueryAcVoucherD = new SData();
            int i = 1;
            foreach (var item in datalist)
            {
                // * 给主键赋值，方便前端选择数据
                item["id"] = i;

                // * 代码、名称
                var group = item.Item<string>(groupkey);
                if (string.IsNullOrEmpty(group))
                {
                    item["dcode"] = string.Empty;
                    item["title"] = string.Empty;
                }
                else
                {
                    item["dcode"] = group.Sp_First();
                    item["title"] = group.Sp_Last();
                }

                // 得到查询条件
                newQueryAcVoucherD = GetQueryVoucherDSub(qParm, groupkey, item["dcode"].ToString());

                // * 计算借方勾对金额、借方未勾对金额
                var amountd = Convert.ToDecimal(item["sh.a_sum_amountd"]); // 借方金额
                item["amountd"] = amountd; // 借方金额
                newQueryAcVoucherD["AcVoucherDSub.cd"] = AcConst.Debit;
                var amountdreco = GetRecoAmount(newQueryAcVoucherD);
                var amountdunreco = amountd - amountdreco;
                item["amountdreco"] = amountdreco; // 借方勾对金额
                item["amountdunreco"] = amountdunreco; // 借方未勾对金额

                // * 计算贷方勾对金额、贷方未勾对金额
                var amountc = Convert.ToDecimal(item["sh.a_sum_amountc"]); // 贷方金额
                item["amountc"] = amountc; // 贷方金额
                newQueryAcVoucherD["AcVoucherDSub.cd"] = AcConst.Credit;
                var amountcreco = GetRecoAmount(newQueryAcVoucherD);
                var amountcunreco = amountc - amountcreco;
                item["amountcreco"] = amountcreco; // 贷方勾对金额
                item["amountcunreco"] = amountcunreco; // 贷方未勾对金额

                // * 计算余额=【借方金额】-【贷方金额】
                item["amountend"] = amountd - amountc;

                // * 计算应收余额=【借方未勾对金额】-【贷方未勾对金额】
                item["amountrecend"] = amountdunreco - amountcunreco;

                // * 计算应付余额=【贷方未勾对金额】-【借方未勾对金额】
                item["amountpayend"] = amountcunreco - amountdunreco;
                i++;
            }
        }

        /// <summary>
        /// 赋值合计行
        /// </summary>
        /// <param name="datalist">列表数据</param>
        /// <param name="groupkey">分组key</param>
        /// <param name="qParm">查询条件</param>
        private void HandleSum(List<SData> datalist, string groupkey, SData qParm)
        {
            if (datalist.Count > 0)
            {
                // 计算借方合计
                var queryAcVoucherD = qParm.ToJson().ToSDataFromJson();
                queryAcVoucherD["cd"] = AcConst.Debit.ToString();
                var sumAmountD = this.ApiTask.GetBalance(EBalanceType.h, "amountd", queryAcVoucherD.toParmStr());
                var sumd = Convert.ToDecimal(sumAmountD);

                // 计算贷方合计
                queryAcVoucherD["cd"] = AcConst.Credit.ToString();
                var sumAmountC = this.ApiTask.GetBalance(EBalanceType.h, "amountc", queryAcVoucherD.toParmStr());
                var sumc = Convert.ToDecimal(sumAmountC);
                queryAcVoucherD.Remove("cd");

                // * 计算借方已勾对金额、借方未勾对金额
                var newQueryAcVoucherD = GetQueryVoucherDSub(qParm, groupkey, string.Empty);
                newQueryAcVoucherD["acvoucherd.cd"] = AcConst.Debit.ToString();
                var sumdReco = GetRecoAmount(newQueryAcVoucherD); // 借方已勾对金额
                var sumdUnReco = sumd - sumdReco; // 借方未勾对金额

                // * 计算贷方已勾对金额、贷方未勾对金额
                newQueryAcVoucherD["acvoucherd.cd"] = AcConst.Credit.ToString();
                var sumcReco = GetRecoAmount(newQueryAcVoucherD); // 贷方已勾对金额
                var sumcUnReco = sumc - sumcReco; // 贷方未勾对金额

                // * 计算余额=【借方金额】-【贷方金额】
                var sumEnd = sumd - sumc;

                // * 计算应收余额=【借方未勾对金额】-【贷方未勾对金额 】
                var sumRecEnd = sumdUnReco - sumcUnReco;

                // * 计算应付余额=【贷方未勾对金额】-【借方未勾对金额 】
                var sumPayEnd = sumcUnReco - sumdUnReco;

                // 将金额保存在列表第一行中
                datalist[0]["sumd"] = sumd; // 借方金额
                datalist[0]["sumc"] = sumc; // 贷方金额
                datalist[0]["sumdreco"] = sumdReco; // 借方已勾对金额
                datalist[0]["sumdunreco"] = sumdUnReco; // 借方未勾对金额
                datalist[0]["sumcreco"] = sumcReco; // 贷方已勾对金额
                datalist[0]["sumcunreco"] = sumcUnReco; // 贷方未勾对金额
                datalist[0]["sumend"] = sumEnd; // 余额
                datalist[0]["sumrecend"] = sumRecEnd; // 应收余额
                datalist[0]["sumpayend"] = sumPayEnd; // 应付余额
            }
        }

        #endregion
    }
}
