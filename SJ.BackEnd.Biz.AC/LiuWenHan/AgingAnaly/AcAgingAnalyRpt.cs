#region  <<版本注释>>
/* ========================================================== 
// <copyright file="AcAgingAnalyRpt.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcAgingAnalyRpt
 * 1.预测逾期账龄报表统计
 * 2.应收应付逾期账龄报表统计
 * 3.借方贷方逾期账龄报表统计
* 创 建 者：刘文汉
* 创建时间：2020/7/3 17:18:12
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using SJ.BackEnd.Base;
using SJ.Global;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 账龄分析报表实体类
    /// 1.描述:账龄类报表统计实体
    /// 预测逾期账龄报表统计
    /// 应收应付逾期账龄报表统计
    /// 借方贷方逾期账龄报表统计
    /// 2.约定:传参lastdata=1 返回分页信息和账龄信息 不传不会返回
    /// 3.业务逻辑:
    /// 查询数据前：根据分组选择调用AcBalance或者AcAnalySet
    /// 查询数据后：计算请求字段的金额
    /// </summary>
    [ClassData("cname", "账龄报表", "vision", 1)]
    public class AcAgingAnalyRpt : BizQuery
    {
        // 返回的数据 包含报表数据和分页数据
        private SData rtValue = new SData("data", new List<SData>(), "pager", new SData("rowCount", 0, "PageCount", 0));

        // 页数
        private int pageIndex = 1;

        // 每页显示数据条数
        private int pageSize = 10;

        // 需要保存的额外数据 包含科目对账,账龄,和明细的查询参数等
        private SData bizDatas = new SData();

        /// <summary>
        /// 初始化字段，查询参数
        /// </summary>
        protected override void OnCustomDefine()
        {
            this.AddParm("lastdata", "是否返回分页和账龄", EFieldType.整数);
            this.AddParm("acanalyset", "账龄分析时间设置", RefBiz: "AcAnalySet"); // 关联账龄分析时间设置实体
            this.AddParm("acledgers", "明细账", RefBiz: "AcLedgers"); // 关联的明细账实体
            this.AddParm("acsubrec", "科目对账", RefBiz: "AcSubRec"); // 关联的科目对账实体
            this.AddParm("summaryitem", "汇总项目");    // 可以根据账龄或者汇总项进行分组统计
            this.AddParm("account.billtype", "单据类型"); // 统计收付款单类型参数,参照应收应付逾期分析
            this.AddParm("account.flow", "余额方向");    // 根据科目的余额方向统计,参照借贷方逾期账龄分析
            this.AddParm("analysdate", "分析日期"); // 分析日期

            //// 定义字段 可通过 rec.a_sum_字段名称 聚合的方式来统计
            this.AddField("rec.a_sum_amount", "应收余额", EFieldType.数值, RealField: "dcode");
            this.AddField("pay.a_sum_amount", "应付余额", EFieldType.数值, RealField: "dcode");
            this.AddField("rec.a_sum_amountd", "应收借方金额", EFieldType.数值, RealField: "dcode");
            this.AddField("pay.a_sum_amountd", "应付借方金额", EFieldType.数值, RealField: "dcode");
            this.AddField("rec.a_sum_amountc", "应收贷方金额", EFieldType.数值, RealField: "dcode");
            this.AddField("pay.a_sum_amountc", "应付贷方金额", EFieldType.数值, RealField: "dcode");
            this.AddField("rec.a_sum_tickamountd", "应收借方勾对金额", EFieldType.数值, RealField: "dcode");
            this.AddField("pay.a_sum_tickamountd", "应付借方勾对金额", EFieldType.数值, RealField: "dcode");
            this.AddField("rec.a_sum_tickamountc", "应收贷方勾对金额", EFieldType.数值, RealField: "dcode");
            this.AddField("pay.a_sum_tickamountc", "应付贷方勾对金额", EFieldType.数值, RealField: "dcode");
            this.AddField("rec.a_sum_untickamountd", "应收借方未勾对金额", EFieldType.数值, RealField: "dcode");
            this.AddField("pay.a_sum_untickamountd", "应付借方未勾对金额", EFieldType.数值, RealField: "dcode");
            this.AddField("rec.a_sum_untickamountc", "应收贷方未勾对金额", EFieldType.数值, RealField: "dcode");
            this.AddField("a_sum_untickamountd", "借方未勾对金额", EFieldType.数值, RealField: "dcode");
            this.AddField("a_sum_untickamountc", "贷方未勾对金额", EFieldType.数值, RealField: "dcode");
            this.AddField("pay.a_sum_untickamountc", "应付贷方未勾对金额", EFieldType.数值, RealField: "dcode");

            // 累计金额，差额,累计差额只会在按照账龄分组,查询预测和逾期账龄时显示
            this.AddField("rec.a_sum_camountd", "应收借方累计金额", EFieldType.数值, RealField: "dcode");
            this.AddField("pay.a_sum_camountd", "应付借方累计金额", EFieldType.数值, RealField: "dcode");
            this.AddField("rec.a_sum_camountc", "应收贷方累计金额", EFieldType.数值, RealField: "dcode");
            this.AddField("pay.a_sum_camountc", "应付贷方累计金额", EFieldType.数值, RealField: "dcode");

            // 统计应收应付逾期时候可以返回
            this.AddField("totaldue", "到期合计", EFieldType.数值, RealField: "dcode");
            this.AddField("totalundue", "未到期合计", EFieldType.数值, RealField: "dcode");
        }

        #region 框架重写方法

        /// <summary>
        /// 初始化sql
        /// </summary>
        /// <param name="sql">查询sql</param>
        protected override void OnInitBaseSql(ref DBSql sql)
        {
            sql = new DBSql
            {
                SqlFrom = new SData("m", "account m"),
                SqlWhere = "m.id = -1",
                SqlOrderBy = "m.id",
            };
        }

        /// <summary>
        /// 初始定义未做业务,子类必须实现
        /// </summary>
        protected override void OnInitDefine()
        {
        }

        /// <summary>
        /// 查询数据前
        /// 1.根据不同分组查询不同的实体
        ///   按照账龄分组查询账龄
        ///   其他分组查询AcBalance
        /// </summary>
        /// <param name="data">查询参数</param>
        protected override void OnGetListBefore(SData data)
        {
            // 获取分页参数，处理核算单位
            InitQueryParms(data);

            // 分页数据
            var pager = rtValue.Item<SData>("pager");

            // 获取分组项，只支持单个分组项 没填默认根据科目分组
            var groupBy = string.IsNullOrWhiteSpace(data.Item<string>("summaryitem")) ? string.Empty : data.Item<string>("summaryitem").Sp_First().Replace("-", ".");

            // 如果不存在分组直接返回
            if (string.IsNullOrWhiteSpace(groupBy))
            {
                bizDatas.Add("datalist.last", new SData("pager", pager, "acanalyset", new List<SData>()));
                return;
            }

            // 判断是否存在该分组项，不存在直接返回。
            // 获取标准的汇总项
            var allGroupKeys = ApiTask.BalanceGroupKeys();
            allGroupKeys.Add(new SData("dcode", nameof(AcAnalySet).ToLower()));
            var groupByKey = allGroupKeys.FirstOrDefault(x => x.Item<string>("dcode").Equals(groupBy));
            if (groupByKey == null)
            {
                bizDatas.Add("datalist.last", new SData("pager", pager, "acanalyset", new List<SData>()));
                return;
            }

            // 解析关联实体，查询出科目对账和账龄的数据
            ResolvRefQParms(data, groupBy);

            // 如果不是指定查询一个科目对账数据，直接返回
            var acsubrec = (bizDatas["acsubrec"] as SData)["data"] as List<SData>;

            bizDatas["groupbykey"] = groupByKey["dcode"];

            // 约定，在返回数据集的最后一行加上返回的分页信息和其他附属信息 确定数据返回格式
            if (groupBy.ToLower().Equals(nameof(AcAnalySet).ToLower()))
            {
                bizDatas.Add("datalist.last", new SData("pager", pager));
            }
            else
            {
                bizDatas.Add("datalist.last", new SData("pager", pager, "acanalyset", new List<SData>()));
            }

            // 不支持多个科目对账的报表统计
            if (acsubrec.Count != 1)
            {
                return;
            }

            // 默认查询科目对账下面所有的科目
            var queryAccount = new SData("SubRec", acsubrec[0].Item<string>("dcode"), "account", "(1)");

            // 单据类型
            queryAccount["billtype"] = data.Item<string>("account.billtype");

            // 科目的余额方向
            queryAccount["account.flow"] = data.Item<string>("account.flow");

            // 指定科目
            queryAccount["account"] = data.Item<string>("acledgers.account");

            bizDatas["account"] = ApiTask.Biz<BizTable>(nameof(AcReconRef)).GetListData(0, 1, queryAccount.toParmStr(), "account,billtype").ToList();

            // 如果科目对账没有查询出科目,直接返回
            if ((bizDatas["account"] as List<SData>).Count <= 0)
            {
                return;
            }

            // 添加科目对账的科目条件
            var accountList = ((List<SData>)bizDatas["account"]).Select(x => x["account"].ToString().Sp_First()).ToList();
            var accountListStr = string.Join(",", accountList);

            ((SData)bizDatas["acledgers_query"])["account"] = accountList.Count > 0 ? accountListStr : "[-1]";

            // 获取当前系统参数：按核算期进行账龄分析
            var auseperiod = ApiTask.GetParms("auseperiod").Equals("1");

            bizDatas["auseperiod"] = auseperiod;

            DateTime analysisdate = DateTime.MinValue;

            // 如果按照到期日期统计不传分析日期直接返回
            if (!auseperiod && (string.IsNullOrWhiteSpace(data.Item<string>("analysdate")) || !DateTime.TryParse(data.Item<string>("analysdate"), out analysisdate)))
            {
                return;
            }

            bizDatas["analysdate"] = analysisdate; // 到期日期

            // 计算账龄的区间范围
            CalculationAcAnalySet(auseperiod, analysisdate);

            // 取出账龄设置数据
            var setData = bizDatas["acanalyset"] as SData;
            var acAnalySet = setData.Item<List<SData>>("data");

            // 初始化数据结构
            rtValue["data"] = InitDataStruct(groupBy, acAnalySet, auseperiod, ref pager);
        }

        /// <summary>
        /// 查询数据后
        /// 1.对统计字段查询赋值
        /// </summary>
        /// <param name="qParms">查询参数</param>
        /// <param name="datas">查询数据</param>
        protected override void OnGetListAfter(SData qParms, List<SData> datas)
        {
            // 如果没有取到数据直接返回
            bool lastdata = qParms.Item<bool>("lastdata");

            if (rtValue.Item<List<SData>>("data").Count <= 0)
            {
                // 如果需要显示分页或者附加信息则添加
                if (lastdata)
                {
                    datas.Add(bizDatas.Item<SData>("datalist.last"));
                }

                return;
            }

            // 要查询统计的字段
            if (!string.IsNullOrWhiteSpace(qParms.Item<string>("fields")))
            {
                // 查询聚合字段并赋值
                EvalFields(qParms);
            }

            var datasList = rtValue.Item<List<SData>>("data");

            // 约定，在返回数据集的最后一行加上返回的分页信息和其他附属信息
            if (lastdata)
            {
                datasList.Add(bizDatas.Item<SData>("datalist.last"));
            }

            datas.AddRange(datasList);
        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 处理查询参数
        /// 1.获取分页数据
        /// 2.处理核算单位
        /// </summary>
        /// <param name="qParms">查询参数</param>
        protected void InitQueryParms(SData qParms)
        {
            // 处理url 参数 获取pageindex 和 pagesize
            int tempInt = 0;

            // 获取pageindex
            if (int.TryParse(qParms.Item<string>("pageindex"), out tempInt))
            {
                pageIndex = tempInt;
            }

            // 获取pagesize
            if (int.TryParse(qParms.Item<string>("pagesize"), out tempInt))
            {
                pageSize = tempInt;
            }

            // 处理核算单位
            this.ApiTask.SubUnitProcess(qParms);
        }

        /// <summary>
        /// 解析关联查询
        /// 1.根据关联查询参数查询出关联实体数据
        /// 2.数据存放在bizDatas属性中
        /// </summary>
        /// <param name="qParms">查询参数</param>
        /// <param name="groupBy">当前分组</param>
        protected void ResolvRefQParms(SData qParms, string groupBy)
        {
            // 查询账龄，科目对账
            foreach (var bizField in this.ListParm.Where(x => !string.IsNullOrWhiteSpace(((FieldDefine)x.Value).RefBiz)))
            {
                // 封装查询参数条件
                var queryParms = new SData();

                // 组装要查询的关联实体的查询参数
                foreach (var query in qParms.Where(x => x.Key.StartsWith(bizField.Key)))
                {
                    string key = query.Key.Contains($"{bizField.Key}.") ? query.Key.Replace($"{bizField.Key}.", string.Empty) : "dcode";
                    queryParms.Add(key, query.Value);
                }

                // 按照账龄分组的searchstring条件 是过滤 账龄的
                if (groupBy.ToLower().Equals(nameof(AcAnalySet).ToLower()) && bizField.Key.Equals("acanalyset"))
                {
                    queryParms["searchstring"] = qParms.Item<string>("searchstring");
                }

                // 按照汇总项目分组的 searchstring条件 是 凭证的过滤条件
                else if (!groupBy.ToLower().Equals(nameof(AcAnalySet).ToLower()) && bizField.Key.Equals("acledgers"))
                {
                    queryParms["searchstring"] = qParms.Item<string>("searchstring");
                }

                // 存储查询各个实体的查询参数
                bizDatas.Add(bizField.Key + "_query", queryParms);
                if (bizField.Key.Equals("acledgers"))
                {
                    continue;
                }

                // 存储关联实体的查询数据
                bizDatas.Add(bizField.Key, ApiTask.Biz<BizTable>(((FieldDefine)bizField.Value).RefBiz).GetList(0, 1, queryParms.toParmStr()));
            }
        }

        /// <summary>
        /// 计算账龄的时间区间
        /// </summary>
        private void CalculationAcAnalySet(bool auseperiod, DateTime analysisdate)
        {
            var setData = bizDatas["acanalyset"] as SData;

            // 账龄时间设置
            var acAnalySet = setData.Item<List<SData>>("data");

            // 计算账龄的区间范围
            if (auseperiod)
            {
                // 提前计算好账龄的起始时间区间
                foreach (var set in acAnalySet)
                {
                    // 按照核算期
                    // 获取当前核算期
                    var CurrentPeriod = Convert.ToInt32(ApiTask.GetParms(AcConst.PeriodCurrent));

                    // 获取每年核算期
                    var PeriodNum = Convert.ToInt32(ApiTask.GetParms(AcConst.PeriodNum));

                    // 计算起始核算期
                    AnalyDateHelper.GetAnalyDate(CurrentPeriod, set, out var startDate, out var endDate, PeriodNum);

                    // 起始时间
                    set["startdate"] = startDate;

                    // 结束时间
                    set["enddate"] = endDate;

                    // 区间描述
                    set["datedesc"] = $"{startDate} {"至"} {endDate}";

                    // 预测账龄与预期账龄的 时间区间表达方式不一样
                    if (set.Item<string>("acanalytype").Sp_First().Equals(AcConst.Overdue))
                    {
                        var isLeftInterval = set.Item<int>("ends") == 999; // 只有结束日期
                        if (isLeftInterval)
                        {
                            set["startdate"] = null;

                            // 区间描述
                            set["datedesc"] = $"{AnalyDateHelper.AddPeriod(endDate, 1, PeriodNum)}之前";
                        }
                    }
                    else
                    {
                        var isRightInterval = set.Item<int>("ends") == 999; // 这个时候只有起始日期
                        if (isRightInterval)
                        {
                            set["enddate"] = null;

                            // 区间描述
                            set["datedesc"] = $"{AnalyDateHelper.AddPeriod(startDate, -1, PeriodNum)}之后";
                        }
                    }

                    // 查询的区间(转换成201901:201912形式)
                    set["start_end"] = $"{(string.IsNullOrWhiteSpace(set.Item<string>("startdate")) ? string.Empty : set.Item<string>("startdate"))}:{(string.IsNullOrWhiteSpace(set.Item<string>("enddate")) ? string.Empty : set.Item<string>("enddate"))}";
                    set["acledgers_query"] = $"period={set["start_end"]}";
                }
            }
            else
            {
                // 提前计算好账龄的起始时间区间
                foreach (var set in acAnalySet)
                {
                    // 按照分析日期
                    AnalyDateHelper.GetAnalyDate(analysisdate, set, out var startDate, out var endDate);
                    set["startdate"] = $"{startDate:yyyy-MM-dd}";
                    set["enddate"] = $"{endDate:yyyy-MM-dd}";
                    set["datedesc"] = $"{startDate:yyyy-MM-dd} {"至"} {endDate:yyyy-MM-dd}";

                    // 预测账龄与预期账龄的 时间区间表达方式不一样
                    if (set.Item<string>("acanalytype").Sp_First().Equals(AcConst.Overdue))
                    {
                        var isLeftInterval = set.Item<int>("ends") == 999; // 只有结束日期
                        if (isLeftInterval)
                        {
                            set["startdate"] = null;
                            set["datedesc"] = $"{endDate.AddDays(1):yyyy-MM-dd}之前";
                        }
                    }
                    else
                    {
                        var isRightInterval = set.Item<int>("ends") == 999; // 这个时候只有起始日期
                        if (isRightInterval)
                        {
                            set["enddate"] = null;
                            set["datedesc"] = $"{startDate.AddDays(-1):yyyy-MM-dd}之后";
                        }
                    }

                    // 查询的区间(转换成2019-12-01:2019-12-02形式)
                    set["start_end"] = $"{(string.IsNullOrWhiteSpace(set.Item<string>("startdate")) ? string.Empty : set.Item<string>("startdate"))}:{(string.IsNullOrWhiteSpace(set.Item<string>("enddate")) ? string.Empty : set.Item<string>("enddate"))}";
                    set["acledgers_query"] = $"edate={set["start_end"]}";
                }
            }
        }

        /// <summary>
        /// 给定义字段赋值
        /// </summary>
        /// <param name="qParms">查询参数</param>
        private void EvalFields(SData qParms)
        {
            // 取出科目对账数据
            var acsubrec = (bizDatas["acsubrec"] as SData)["data"] as List<SData>;

            // 要查询的字段
            List<string> fields = qParms.Item<string>("fields").Split(',').ToList();

            var recSumKey = "rec.a_sum_";
            var paySumKey = "pay.a_sum_";
            var sumkey = "a_sum_";

            // 应收应付统计
            var recFields = fields.Where(x => x.StartsWith("rec.a_sum_") || x.StartsWith("pay.a_sum_") || x.StartsWith("a_sum_")).ToList();

            // 科目对账的科目
            var accounts = bizDatas["account"] as List<SData>;

            var accountsStr = string.Join(",", accounts.Select(x => x["account"].ToString().Sp_First()));

            // 收款科目
            var skAccountList = accounts.Where(x => x["billtype"]?.ToString().Sp_First() == AcConst.ReceiptVoucher).Select(x => x["account"].ToString().Sp_First()).ToList();
            var skAccount = string.Join(",", skAccountList);

            // 付款科目
            var fkAccountList = accounts?.Where(x => x["billType"]?.ToString().Sp_First() == AcConst.PayVoucher).Select(x => x["account"].ToString().Sp_First()).ToList();
            var fkAccount = string.Join(",", fkAccountList);

            // 得到数据后，求聚合字段
            foreach (var row in rtValue["data"] as List<SData>)
            {
                var queryParms = ((SData)bizDatas["acledgers_query"]).ToJson().ToSDataFromJson(); // 深拷贝

                // 添加分组 的 条件 (根据账龄或者汇总项)
                if (bizDatas.Item<string>("groupbykey").Equals(nameof(AcAnalySet).ToLower()))
                {
                    var key = row.Item<string>("acledgers_query").Sp_First("=");
                    var value = row.Item<string>("acledgers_query").Sp_Last("=");
                    queryParms[key] = value;
                }
                else
                {
                    // 根据汇总项目进行分组的
                    queryParms[bizDatas.Item<string>("groupbykey")] = string.IsNullOrWhiteSpace(row.Item<string>("dcode")) ? "(0)" : row.Item<string>("dcode");

                    // 如果是应收应付逾期分析，则统计未到期和到期金额
                    if (!string.IsNullOrWhiteSpace(qParms.Item<string>("account.billtype")) ||
                        !string.IsNullOrWhiteSpace(qParms.Item<string>("account.flow")))
                    {
                        var setData = bizDatas["acanalyset"] as SData;

                        // 统计借方金额
                        bool jfje = (!string.IsNullOrWhiteSpace(qParms.Item<string>("account.billtype")) && qParms.Item<string>("account.billtype").Sp_First().Equals(AcConst.ReceiptVoucher)) || (!string.IsNullOrWhiteSpace(qParms.Item<string>("account.flow")) && !qParms.Item<bool>("account.flow"));

                        // 账龄时间设置
                        var acAnalySet = setData.Item<List<SData>>("data");

                        // 统计账龄的未勾对金额
                        if (bizDatas.Item<bool>("auseperiod"))
                        {
                            foreach (var set in acAnalySet)
                            {
                                var queryAc = queryParms.ToJson().ToSDataFromJson();
                                queryAc["period"] = set["start_end"];

                                // 应收借方金额
                                decimal rec_a_sum_amountd = 0;

                                // 应收借方勾对金额
                                decimal rec_a_sum_tickamountd = 0;
                                rec_a_sum_amountd = Convert.ToDecimal(ApiTask.Biz<BizTable>(nameof(AcLedgers)).GetAggregate(queryAc.toParmStr(), EAggregate.Sum, jfje ? "amountd" : "amountc") ?? 0);

                                GetTickAmount($"[{acsubrec[0]["id"]}]", "acvoucherdsub", queryAc, jfje ? AcConst.Debit : AcConst.Credit, ref rec_a_sum_tickamountd);

                                // 未勾对金额=合计金额-已勾对金额
                                var rec_a_sum_untickamountd = rec_a_sum_amountd - rec_a_sum_tickamountd;
                                row[$"acanalyset.{set["dcode"]}"] = rec_a_sum_untickamountd;
                            }

                            // 统计到期和未到期
                            var queryTo = queryParms.ToJson().ToSDataFromJson();

                            // 获取当前核算期
                            var CurrentPeriod = Convert.ToInt32(ApiTask.GetParms(AcConst.PeriodCurrent));

                            // 获取每年核算期
                            var PeriodNum = Convert.ToInt32(ApiTask.GetParms(AcConst.PeriodNum));

                            queryTo["period"] = $":{AnalyDateHelper.AddPeriod(CurrentPeriod, -1, PeriodNum)}";
                            row["totaldue"] = GetUnTickAmount($"[{acsubrec[0]["id"]}]", "acvoucherdsub", jfje ? AcConst.Debit : AcConst.Credit, queryTo);
                            queryTo["period"] = $"{CurrentPeriod}:";
                            row["totalundue"] = GetUnTickAmount($"[{acsubrec[0]["id"]}]", "acvoucherdsub", jfje ? AcConst.Debit : AcConst.Credit, queryTo);

                        }
                        else
                        {
                            foreach (var set in acAnalySet)
                            {
                                var queryAc = queryParms.ToJson().ToSDataFromJson();
                                queryAc["edate"] = set["start_end"];

                                // 应收借方金额
                                decimal rec_a_sum_amountd = 0;

                                // 应收借方勾对金额
                                decimal rec_a_sum_tickamountd = 0;
                                rec_a_sum_amountd = Convert.ToDecimal(ApiTask.Biz<BizTable>(nameof(AcLedgers)).GetAggregate(queryAc.toParmStr(), EAggregate.Sum, jfje ? "amountd" : "amountc") ?? 0);

                                GetTickAmount($"[{acsubrec[0]["id"]}]", "acvoucherdsub", queryAc, jfje ? AcConst.Debit : AcConst.Credit, ref rec_a_sum_tickamountd);

                                // 未勾对金额=合计金额-已勾对金额
                                var rec_a_sum_untickamountd = rec_a_sum_amountd - rec_a_sum_tickamountd;
                                row[$"acanalyset.{set["dcode"]}"] = rec_a_sum_untickamountd;
                            }

                            // 统计到期和未到期
                            var queryTo = queryParms.ToJson().ToSDataFromJson();

                            // 当前分析日期
                            var analysisdate = bizDatas.Item<DateTime>("analysdate");
                            queryTo["edate"] = $":{analysisdate.AddDays(-1):yyyy-MM-dd}";
                            row["totaldue"] = GetUnTickAmount($"[{acsubrec[0]["id"]}]", "acvoucherdsub", jfje ? AcConst.Debit : AcConst.Credit, queryTo);
                            queryTo["edate"] = $"{analysisdate:yyyy-MM-dd}:";
                            row["totalundue"] = GetUnTickAmount($"[{acsubrec[0]["id"]}]", "acvoucherdsub", jfje ? AcConst.Debit : AcConst.Credit, queryTo);
                        }
                    }
                }

                foreach (var recFieldt in recFields)
                {
                    var queryParmsCell = queryParms.ToJson().ToSDataFromJson();

                    string currentSumKey = sumkey;
                    if (recFieldt.StartsWith(paySumKey))
                    {
                        currentSumKey = paySumKey;
                    }
                    else if (recFieldt.StartsWith(recSumKey))
                    {
                        currentSumKey = recSumKey;
                    }

                    var recField = recFieldt.Replace(currentSumKey, string.Empty);

                    // 字段在定义中才赋值
                    if (!string.IsNullOrEmpty(this.ListField.FirstOrDefault(x => x.Key.EndsWith(recField)).Key))
                    {
                        // 收款科目或者付款科目条件
                        if (currentSumKey.Equals(recSumKey))
                        {
                            AndForPop(queryParmsCell, "acaccount", "account", skAccountList.Count > 0 ? skAccount : "[-1]");
                        }
                        else if (currentSumKey.Equals(paySumKey))
                        {
                            AndForPop(queryParmsCell, "acaccount", "account", fkAccountList.Count > 0 ? fkAccount : "[-1]");
                        }
                        else
                        {
                            AndForPop(queryParmsCell, "acaccount", "account", accounts.Count > 0 ? accountsStr : "[-1]");
                        }

                        // 明细账聚合的字段
                        if (recField.Equals("amount") || recField.Equals("amountd") || recField.Equals("amountc"))
                        {
                            // 没有赋值过才需要赋值
                            if (!row.ContainsKey($"{currentSumKey}{recField}"))
                            {
                                row[$"{currentSumKey}{recField}"] = Convert.ToDecimal(ApiTask.Biz<BizTable>(nameof(AcLedgers)).GetAggregate(queryParmsCell.toParmStr(), EAggregate.Sum, recField) ?? 0);
                            }
                        }

                        // 勾对的聚合字段
                        else if (recField.Equals("tickamountd") || recField.Equals("tickamountc"))
                        {
                            int cd = recField.EndsWith("d") ? AcConst.Debit : AcConst.Credit;
                            decimal tickamount = 0;
                            GetTickAmount($"[{acsubrec[0]["id"]}]", "acvoucherdsub", queryParmsCell, cd, ref tickamount);

                            if (!row.ContainsKey($"{currentSumKey}{recField}"))
                            {
                                row[$"{currentSumKey}{recField}"] = tickamount;
                            }
                        }

                        // 借贷方的未勾对金额
                        else if (recField.Equals("untickamountd") || recField.Equals("untickamountc"))
                        {
                            // 借方未勾对金额
                            if (recField.Equals("untickamountd"))
                            {
                                // 应收借方金额
                                decimal rec_a_sum_amountd = 0;

                                // 应收借方勾对金额
                                decimal rec_a_sum_tickamountd = 0;

                                // 如果取过数据直接从行数据去取
                                if (row.ContainsKey($"{currentSumKey}amountd"))
                                {
                                    rec_a_sum_amountd = row.Item<decimal>($"{currentSumKey}amountd");
                                }
                                else
                                {
                                    rec_a_sum_amountd = Convert.ToDecimal(ApiTask.Biz<BizTable>(nameof(AcLedgers)).GetAggregate(queryParmsCell.toParmStr(), EAggregate.Sum, "amountd") ?? 0);
                                    if (recFields.Contains("amountd"))
                                    {
                                        row[$"{currentSumKey}amountd"] = rec_a_sum_amountd;
                                    }
                                }

                                if (row.ContainsKey($"{currentSumKey}tickamountd"))
                                {
                                    rec_a_sum_tickamountd = row.Item<decimal>($"{currentSumKey}tickamountd");
                                }
                                else
                                {
                                    GetTickAmount($"[{acsubrec[0]["id"]}]", "acvoucherdsub", queryParmsCell, AcConst.Debit, ref rec_a_sum_tickamountd);
                                    if (recFields.Contains("tickamountd"))
                                    {
                                        row[$"{currentSumKey}tickamountd"] = rec_a_sum_tickamountd;
                                    }
                                }

                                // 未勾对金额=合计金额-已勾对金额
                                var rec_a_sum_untickamountd = rec_a_sum_amountd - rec_a_sum_tickamountd;
                                row[$"{currentSumKey}untickamountd"] = rec_a_sum_untickamountd;
                            }

                            // 贷方未勾对金额
                            if (recField.Equals("untickamountc"))
                            {
                                // 应收贷方金额
                                decimal rec_a_sum_amountc = 0;

                                // 应收贷方勾对金额
                                decimal rec_a_sum_tickamountc = 0;

                                if (row.ContainsKey($"{currentSumKey}amountc"))
                                {
                                    rec_a_sum_amountc = row.Item<decimal>($"{currentSumKey}amountc");
                                }
                                else
                                {
                                    rec_a_sum_amountc = Convert.ToDecimal(ApiTask.Biz<BizTable>(nameof(AcLedgers)).GetAggregate(queryParmsCell.toParmStr(), EAggregate.Sum, "amountc") ?? 0);
                                    if (recFields.Contains("amountc"))
                                    {
                                        row[$"{currentSumKey}amountc"] = rec_a_sum_amountc;
                                    }
                                }

                                if (row.ContainsKey($"{currentSumKey}tickamountc"))
                                {
                                    rec_a_sum_tickamountc = row.Item<decimal>($"{currentSumKey}tickamountc");
                                }
                                else
                                {
                                    GetTickAmount($"[{acsubrec[0]["id"]}]", "acvoucherdsub", queryParmsCell, AcConst.Credit, ref rec_a_sum_tickamountc);
                                    if (recFields.Contains("tickamountc"))
                                    {
                                        row[$"{currentSumKey}tickamountc"] = rec_a_sum_tickamountc;
                                    }
                                }

                                var rec_a_sum_untickamountc = rec_a_sum_amountc - rec_a_sum_tickamountc;
                                row[$"{currentSumKey}untickamountc"] = rec_a_sum_untickamountc;
                            }
                        }

                        // 累计金额(累加字段：应收应付金额的累加)
                        else if (recField.Equals("camountd") || recField.Equals("camountc"))
                        {
                            var queryCAmount = queryParmsCell.ToJson().ToSDataFromJson();

                            var key = row.Item<string>("camount_query").Sp_First("=");
                            var value = row.Item<string>("camount_query").Sp_Last("=");
                            queryCAmount[key] = value;
                            if (recField.Equals("camountd"))
                            {
                                // 应收借方金额
                                decimal rec_a_sum_amountd = 0;

                                // 应收借方勾对金额
                                decimal rec_a_sum_tickamountd = 0;
                                rec_a_sum_amountd = Convert.ToDecimal(ApiTask.Biz<BizTable>(nameof(AcLedgers)).GetAggregate(queryCAmount.toParmStr(), EAggregate.Sum, "amountd") ?? 0);

                                GetTickAmount($"[{acsubrec[0]["id"]}]", "acvoucherdsub", queryCAmount, AcConst.Debit, ref rec_a_sum_tickamountd);

                                // 未勾对金额=合计金额-已勾对金额
                                var rec_a_sum_untickamountd = rec_a_sum_amountd - rec_a_sum_tickamountd;
                                row[$"{currentSumKey}camountd"] = rec_a_sum_untickamountd;
                            }

                            if (recField.Equals("camountc"))
                            {
                                // 应收贷方金额
                                decimal rec_a_sum_amountc = 0;

                                // 应收贷方勾对金额
                                decimal rec_a_sum_tickamountc = 0;

                                rec_a_sum_amountc = Convert.ToDecimal(ApiTask.Biz<BizTable>(nameof(AcLedgers)).GetAggregate(queryCAmount.toParmStr(), EAggregate.Sum, "amountc") ?? 0);
                                GetTickAmount($"[{acsubrec[0]["id"]}]", "acvoucherdsub", queryCAmount, AcConst.Credit, ref rec_a_sum_tickamountc);
                                var rec_a_sum_untickamountc = rec_a_sum_amountc - rec_a_sum_tickamountc;
                                row[$"{currentSumKey}camountc"] = rec_a_sum_untickamountc;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 初始化报表的数据结构
        /// 1.根据不同分组查询
        ///   返回账龄的结构或者返回AcBalance结构
        /// </summary>
        /// <param name="groupBy">当前分组</param>
        /// <param name="acAnalySet">账龄数据</param>
        /// <param name="auseperiod">是否根据核算期进行分析</param>
        /// <param name="pager">分页参数</param>
        /// <returns></returns>
        private List<SData> InitDataStruct(string groupBy, List<SData> acAnalySet, bool auseperiod, ref SData pager)
        {
            var dataList = new List<SData>();

            int k = 1;

            // 按照账龄进行分组
            if (groupBy.ToLower().Equals(nameof(AcAnalySet).ToLower()))
            {
                // 逾期账龄
                var oveAcanlySet = acAnalySet.Where(x => x.Item<string>("acanalytype").Sp_First().Equals(AcConst.Overdue)).ToList();

                // 预测账龄
                var expAcanlySet = acAnalySet.Where(x => x.Item<string>("acanalytype").Sp_First().Equals(AcConst.ToExpire)).ToList();

                // 循环账龄 赋值 dcode 和 title 因为这里是取出了所有账龄，需要进行分页
                foreach (var item in acAnalySet.Skip(pageSize * (pageIndex - 1)).Take(pageSize))
                {
                    var row = new SData();
                    row["id"] = k++;
                    row["dcode"] = item.Item<string>("dcode");
                    row["title"] = item.Item<string>("title");

                    // 返回账龄分析类型
                    row["acanalytype"] = item.Item<string>("acanalytype");

                    // 返回账龄的起始时间和区间描述
                    row["startdate"] = item["startdate"];
                    row["enddate"] = item["enddate"];
                    row["datedesc"] = item["datedesc"];

                    // 要统计的账龄区间
                    var otherAcanalySet = new List<SData>();

                    // 是否是预测账龄分析
                    bool isFor = item.Item<string>("acanalytype").Sp_First().Equals(AcConst.ToExpire);

                    // 是否是逾期账龄分析
                    bool isOver = item.Item<string>("acanalytype").Sp_First().Equals(AcConst.Overdue);

                    if (isOver)
                    {
                        // 查询出当前账龄的位置
                        var currentIndex = oveAcanlySet.IndexOf(item);

                        // 查询出包含当前账龄之后的所有账龄  累计区间
                        otherAcanalySet = acAnalySet.Skip(currentIndex).ToList();
                    }
                    else
                    {
                        // 查询出当前账龄的位置
                        var currentIndex = expAcanlySet.IndexOf(item);

                        // 查询出包含当前账龄之前的所有账龄 累计区间
                        otherAcanalySet = acAnalySet.Take(currentIndex + 1).ToList();
                    }

                    // 累计金额的区间集合
                    var camount_query = otherAcanalySet.Select(x => x.Item<string>("start_end"));

                    if (auseperiod)
                    {
                        // 用于统计累计金额的查询条件
                        row["camount_query"] = $"period={string.Join(",", camount_query)}";

                        // 用于列表查询明细账参数
                        row["acledgers_query"] = $"period={item["start_end"]}";
                    }
                    else
                    {
                        // 用于统计累计金额的查询条件
                        row["camount_query"] = $"edate={string.Join(",", camount_query)}";

                        // 用于列表查询明细账参数
                        row["acledgers_query"] = $"edate={item["start_end"]}";
                    }

                    dataList.Add(row);
                }

                bizDatas["groupbykey"] = nameof(AcAnalySet).ToLower();

                pager = new SData("rowCount", acAnalySet.Count, "PageCount", (acAnalySet.Count + pageSize - 1) / pageSize, "PageIndex", pageIndex);

                // 约定，在返回数据集的最后一行加上返回的分页信息和其他附属信息
                bizDatas["datalist.last"] = new SData("pager", pager);
            }
            else
            {
                var queryBalance = ((SData)bizDatas["acledgers_query"]).ToJson().ToSDataFromJson();
                queryBalance["period"] = AcConst.PeriodEnd;

                // 调用余额表
                var baData = ApiTask.Biz<BizTable>(nameof(AcBalance)).GetList(pageSize, pageIndex, queryBalance.toParmStr(), groupBy + ",sb.a_sum_amount");

                // 循环账龄 赋值 dcode 和 title
                foreach (var item in baData["data"] as List<SData>)
                {
                    var row = new SData();
                    row["id"] = k++;
                    row["dcode"] = item.Item<string>(groupBy).Sp_First();
                    row["title"] = item.Item<string>(groupBy).Sp_Last();
                    dataList.Add(row);
                }

                var rt = from set in acAnalySet
                         select new
                         {
                             dcode = set.Item<string>("dcode"),
                             title = set.Item<string>("title"),
                             startdate = set.Item<string>("startdate"),
                             datedesc = set.Item<string>("datedesc"),
                             enddate = set.Item<string>("enddate"),
                             acledgers_query = set.Item<string>("acledgers_query"),
                         };

                // 分页信息
                pager = baData.Item<SData>("pager");

                // 约定，在返回数据集的最后一行加上返回的分页信息和其他附属信息 
                bizDatas["datalist.last"] = new SData("pager", pager, "acanalyset", rt);
            }

            return dataList;
        }

        /// <summary>
        /// 统计未勾对金额
        /// </summary>
        /// <param name="subRec">科目对账</param>
        /// <param name="subEntity">子查询条件</param>
        /// <param name="cd">借贷</param>
        /// <param name="queryParms">查询条件</param>
        private decimal GetUnTickAmount(string subRec, string subEntity, int cd, SData queryParms)
        {
            // 合计金额
            decimal rec_a_sum_amountc = 0;

            // 已勾对金额
            decimal rec_a_sum_tickamountc = 0;

            // 查询合计金额
            rec_a_sum_amountc = Convert.ToDecimal(ApiTask.Biz<BizTable>(nameof(AcLedgers)).GetAggregate(queryParms.toParmStr(), EAggregate.Sum, cd == AcConst.Debit ? "amountd" : "amountc") ?? 0);

            // 查询已勾对金额
            GetTickAmount(subRec, subEntity, queryParms, cd, ref rec_a_sum_tickamountc);

            // 计算并返回未勾对金额
            var rec_a_sum_untickamountc = rec_a_sum_amountc - rec_a_sum_tickamountc;
            return rec_a_sum_untickamountc;
        }

        /// <summary>
        /// 勾对金额合计
        /// </summary>
        /// <param name="subRec">科目对账id</param>
        /// <param name="subEntity">关联实体名称</param>
        /// <param name="queryParamsValue">分录查询条件</param>
        /// <param name="cd">借贷</param>
        /// <param name="tickAmount">勾对金额</param>
        private void GetTickAmount(string subRec, string subEntity, SData queryParamsValue, int? cd, ref decimal tickAmount)
        {
            var queryTickAmount = new SData();
            queryTickAmount[AcConst.RecoTcode] = subRec;
            queryTickAmount["vstate"] = $"{AcConst.Trial}";
            queryTickAmount[$"{subEntity}.subrecid"] = subRec;
            foreach (var key in queryParamsValue.Keys)
            {
                queryTickAmount[$"{subEntity}.{key}"] = queryParamsValue[key];
            }

            if (!string.IsNullOrWhiteSpace(queryTickAmount.Item<string>($"{subEntity}.bdateb")) || !string.IsNullOrWhiteSpace(queryTickAmount.Item<string>($"{subEntity}.bdatee")))
            {
                queryTickAmount[$"{subEntity}.bdate"] =
                    $"{queryTickAmount[$"{subEntity}.bdateb"] ?? string.Empty}:{queryTickAmount[$"{subEntity}.bdatee"] ?? string.Empty}";
            }

            if (!string.IsNullOrWhiteSpace(queryTickAmount.Item<string>($"{subEntity}.periodb")) || !string.IsNullOrWhiteSpace(queryTickAmount.Item<string>($"{subEntity}.periode")))
            {
                queryTickAmount[$"{subEntity}.period"] =
                    $"{queryTickAmount[$"{subEntity}.periodb"] ?? string.Empty}:{queryTickAmount[$"{subEntity}.periode"] ?? string.Empty}";
            }

            queryTickAmount.Remove($"{subEntity}.bdateb");
            queryTickAmount.Remove($"{subEntity}.bdatee");
            queryTickAmount.Remove($"{subEntity}.periodb");
            queryTickAmount.Remove($"{subEntity}.periode");

            if (cd.HasValue && cd == AcConst.Debit)
            {
                queryTickAmount[$"{subEntity}.cd"] = AcConst.Debit;

                // 借方勾对金额
                tickAmount = Convert.ToDecimal(ApiTask.Biz<BizTable>(nameof(AcRecoD)).GetAggregate(
                                                  queryTickAmount.toParmStr(),
                                                  EAggregate.Sum,
                                                  Field: "amountd") ?? 0);
            }
            else if (cd.HasValue && cd == AcConst.Credit)
            {
                queryTickAmount[$"{subEntity}.cd"] = AcConst.Credit;

                // 贷方勾对金额
                tickAmount = Convert.ToDecimal(ApiTask.Biz<BizTable>(nameof(AcRecoD)).GetAggregate(
                                                   queryTickAmount.toParmStr(),
                                                   EAggregate.Sum,
                                                   Field: "amountc") ?? 0);
            }
            else
            {
                // 借贷勾对合计金额
                tickAmount = Convert.ToDecimal(ApiTask.Biz<BizTable>(nameof(AcRecoD)).GetAggregate(
                                                   queryTickAmount.toParmStr(),
                                                   EAggregate.Sum,
                                                   Field: "recoamount") ?? 0);
            }
        }

        /// <summary>
        /// 给queryparms拼接and条件
        /// </summary>
        /// <param name="s">查询参数</param>
        /// <param name="key">拼接查询key</param>
        /// <param name="value">查询参数值</param>
        /// <returns></returns>
        private SData AndForPop(SData s, string popBiz, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(popBiz))
            {
                if (string.IsNullOrWhiteSpace(s.Item<string>(key)))
                {
                    s[key] = value;
                }
                else
                {
                    // 当前key的 or 条件 只截取pop框的 dcode条件
                    var left_query = s[key].ToString().Contains("[-1]") ? new List<string>() : ApiTask.Biz<BizTable>(popBiz).GetListData(0, 1, new SData("dcode", s[key].ToString()).toParmStr(), "dcode").Select(x => x.Item<string>("dcode"));

                    // 当拼接的and 条件不在 已有条件范围内(如 (a=1 or a=2) and a=3) 始终返回false 其实就是求交集 
                    var right_query = value.Contains("[-1]") ? new List<string>() : ApiTask.Biz<BizTable>(popBiz).GetListData(0, 1, new SData("dcode", value).toParmStr(), "dcode").Select(x => x.Item<string>("dcode"));

                    // 借助linq的查询来过滤条件
                    var mix_query = left_query.Intersect(right_query); // 求交集

                    if (!mix_query.Any())
                    {
                        s[key] = "[-1]";
                    }
                    else
                    {
                        s[key] = string.Join(",", mix_query);
                    }
                }
            }

            return s;
        }

        #endregion
    }
}
