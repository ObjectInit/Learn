#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcYearEndV.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcYearEndV
* 创 建 者：张莉
* 创建时间：2019/9/30 9:24:22
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.Global;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SJ.BackEnd.Biz.Pub;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 结转凭证：
    /// 1 描述：期末结账时将某一账户的余额或差额转入另一科目
    /// 2 约定：
    ///     1.继承AcVoucher，dclass是acvoucher；
    ///     2.分核算单位结转凭证，一个核算单位失败，另外一个不影响；
    ///     3.结转凭证调用GetListData接口；
    ///     4.查询传参type为1表示结转凭证；
    /// 3 业务逻辑：
    ///     新增前：初始化结转凭证头
    ///     新增后：
    ///         1.生成有转出科目性质的结转凭证
    ///         2.生成无转出科目性质的结转凭证
    ///         3.更新结转凭证头的借方合计和贷方合计
    ///     查询前：
    ///         1.校验自动凭证结转、核算期、转出余额比例
    ///         2.筛选需要结转的凭证分录集合
    ///         3.分核算单位添加结转凭证
    ///         4.更新结转说明
    /// </summary>
    [ClassData("cname", "自动凭证结转凭证头", "vision", 1)]
    public class AcYearEndV : AcVoucher
    {
        /// <summary>
        /// 自动凭证结转凭证头共享凭证dclass
        /// </summary>
        public override string DataClass => "acvoucher";

        /// <summary>
        /// 预留字段、参数、子查询定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // * 字段定义
            // 启用预留字段
            this.AddField("yearend", ApiTask.L("自动凭证结转"), EFieldType.关联, nameof(AcYearEnd), "a9");

            // 更改显示名称
            ((FieldDefine)ListField["ddesc"]).DisplayName = ApiTask.L("摘要");
        }

        #region 框架方法重写

        /// <summary>
        /// 分核算单位结转凭证
        /// 参数type为1表示结转凭证
        /// 1、判断自动凭证结转是否存在
        /// 2、根据自动凭证结转id获取核算期、预算方案、凭证类型、摘要
        /// 3、校验核算期
        /// 4、分组转出科目、转入科目数据，校验和计算数据
        /// 5、获取需要结转的核算单位
        /// 6、根据科目、Tcode、币种分组统计需要结转的凭证分录的[期末余额]
        /// 7、分核算单位添加结转凭证
        /// </summary>
        /// <param name="qData">自动凭证结转code.name</param>
        protected override void OnGetListBefore(SData qData)
        {
            // 业务逻辑 **********
            // type为1表示结转凭证
            if (qData.Item<int>("type") == AcConst.YearEndType)
            {
                // * 判断自动凭证结转是否存在
                if (string.IsNullOrEmpty(qData.Item<string>("yearend")))
                {
                    throw new Exception(((FieldDefine)ListField["yearend"]).DisplayName + ApiTask.LEnd(PubLang.NotExists));
                }

                var acYearEndBiz = ApiTask.Biz<BizTableCode>(nameof(AcYearEnd));
                var yearendId = acYearEndBiz.GetIDAuto(qData["yearend"].ToString());
                if (yearendId <= 0)
                {
                    throw new Exception(((FieldDefine)ListField["yearend"]).DisplayName + ApiTask.LEnd(PubLang.NotExists));
                }

                var data = new SData();

                // * 根据自动凭证结转id获取核算期、预算方案、凭证类型、摘要
                var acYearEnd = acYearEndBiz.GetItem(yearendId);
                data["acYearEnd"] = acYearEnd;
                data["yearend"] = qData["yearend"];

                // * 校验核算期
                // 核算期格式不规范（yyyy + 核算期数，如：201904，表示2019年第4个核算期），提示：参考通用提示语
                // 核算期小于当前核算期，提示：核算期不能小于当前核算期
                // 核算期超过每年最大期数，提示：核算期不能大于每年最大核算期数
                // 核算期超过当前核算期 + 最大跨区间数【系统参数中设置的凭证录入最大跨区间数】，提示：核算期不能大于最大跨区间数
                PeriodHelper.Check(int.Parse(acYearEnd["period"].ToString()), this.ApiTask);

                // * 分组转出科目、转入科目数据，校验和计算数据

                // 按照(有转出科目性质)转出科目分组求结转余额比例和
                var proAccList = new List<SData>();

                // 根据(无转出科目性质)转出科目分组求结转余额比例和
                var outaccList = new List<SData>();

                // 根据(有转出科目性质)转出科目性质、转出科目、转入科目分组求结转余额比例和
                var proInAccList = new List<SData>();

                // 根据(无转出科目性质)转出科目性质、转出科目、转入科目分组求结转余额比例和
                var inaccList = new List<SData>();

                GetAccountList(yearendId, ref proAccList, ref outaccList, ref proInAccList, ref inaccList);

                // 同一个转出科目的结转余额比例和不等于100 %，提示：同一转出科目的转出余额比例不等于100 %，不允许结转
                var outacc = string.Empty;
                var propertyAccList = CheckRate(proAccList, new SData("exName", "转出科目性质", "keyName", "p_accountId"), ref outacc);
                var outAccList = CheckRate(outaccList, new SData("exName", "转出科目", "keyName", "outacc_id"), ref outacc);

                // 去掉最后一个逗号
                if (!string.IsNullOrEmpty(outacc))
                {
                    outacc = outacc.Substring(0, outacc.LastIndexOf(",", StringComparison.Ordinal));
                }

                // * 获取需要结转的核算单位
                // 获取核算单位
                var subunitAllData = ApiTask.Biz<AcSubunit>(nameof(AcSubunit)).GetListData(0, 1, string.Empty, "id,dcode,title");

                if (subunitAllData.Count > 0)
                {
                    var subunitStr = string.Join(",", subunitAllData.Select(x => x["id"]).ToList());

                    // * 根据科目、Tcode、币种分组统计需要结转的凭证分录的[期末余额]
                    var voucherDList = GetVoucherDList(acYearEnd, outacc, subunitStr);

                    if (voucherDList.Count > 0)
                    {
                        // 结转失败信息
                        var error = string.Empty;

                        // * 分核算单位添加结转凭证
                        foreach (var subunit in subunitAllData)
                        {
                            var amountNotZeroList = voucherDList.Where(x => !string.IsNullOrEmpty(x.Item<string>("subunit__id")) && subunit["id"].ToString() == x.Item<string>("subunit__id")).ToList();

                            if (amountNotZeroList.Count > 0)
                            {
                                // 添加结转凭证
                                var yearendV = ApiTask.Biz<AcYearEndV>(nameof(AcYearEndV));
                                data["proInAccList"] = proInAccList;
                                data["inaccList"] = inaccList;

                                // data["voucherDList"] = amountNotZeroList;
                                data["subunit"] = subunit["dcode"];
                                data["subunit.id"] = subunit["id"];

                                // （有转出科目性质）转出科目的所有结转凭证
                                data["propertyVDList"] = amountNotZeroList.Where(x => propertyAccList.Contains(x["account__id"].ToString())).ToList();

                                // （无转出科目性质）转出科目的所有结转凭证
                                data["outVDList"] = amountNotZeroList.Where(x => outAccList.Contains(x["account__id"].ToString())).ToList();

                                try
                                {
                                    yearendV.Insert(data);
                                }
                                catch (Exception e)
                                {
                                    error += $"({subunit["title"]}){ApiTask.LEnd("核算单位结转失败")}：{e.Message}";
                                }
                            }
                        }

                        // 核算单位结转失败原因
                        if (!string.IsNullOrEmpty(error))
                        {
                            throw new Exception(error);
                        }
                    }
                }
                else
                {
                    throw new Exception(ApiTask.LEnd("转出科目没有需要结转的余额数据"));
                }

                qData["id"] = -1;
            }
        }

        /// <summary>
        /// 结转凭证：
        /// 1、初始化结转凭证头
        /// </summary>
        /// <param name="data">结转凭证头实体</param>
        protected override void OnInsertBefore(SData data)
        {
            // 判断自动凭证结转是否存在
            if (data?["yearend"] == null)
            {
                throw new Exception(((FieldDefine)ListField["yearend"]).DisplayName + ApiTask.LEnd(PubLang.NotExists));
            }

            // 数据准备 **********
            // 初始化结转凭证头
            InitYearEndVoucher(data, data.Item<SData>("acYearEnd"));
        }

        /// <summary>
        /// 结转凭证头新增后的逻辑：
        /// 1、生成有转出科目性质的结转凭证
        /// 2、生成无转出科目性质的结转凭证
        /// 3、更新结转凭证头的借方合计和贷方合计
        /// </summary>
        /// <param name="data">要新增的结转凭证体数据</param>
        protected override void OnInsertAfter(SData data)
        {
            // 业务逻辑 **********
            var propertyVDList = data.Item<List<SData>>("propertyVDList");
            var outVDList = data.Item<List<SData>>("outVDList");
            var proInAccList = data.Item<List<SData>>("proInAccList");
            var inaccList = data.Item<List<SData>>("inaccList");

            // 获取所有定义的转入科目相关信息
            var query = new SData("yearend", data["yearend"], "inacc", "(1)");
            var yearEndAList = ApiTask.Biz<BizTableCode>(nameof(AcYearEndA)).GetListData(0, 1, query.toParmStr());

            // * 生成有转出科目性质的结转凭证
            if (propertyVDList.Count > 0)
            {
                CreateVoucherD(propertyVDList, proInAccList, data, yearEndAList);
            }

            // * 生成无转出科目性质的结转凭证
            if (outVDList.Count > 0)
            {
                CreateVoucherD(outVDList, inaccList, data, yearEndAList);
            }

            // * 更新结转凭证头的借方合计和贷方合计
            int.TryParse(data["id"]?.ToString(), out int voucherId);
            if (voucherId > 0)
            {
                UpdateVoucherSumdAndSumc(voucherId);
            }
        }
        #endregion

        #region 自定义方法

        /// <summary>
        /// 校验转出科目性质或转出科目的余额比例100%，并获取转出科目集合
        /// </summary>
        /// <param name="accountList">转出科目、比例集合</param>
        /// <param name="param">特殊参数exName（转出科目性质或转出科目）keyName（p_accountId或outacc_id）</param>
        /// <param name="outacc">转出科目集合</param>
        /// <returns>转出科目性质下科目集合或转出科目集合</returns>
        private List<string> CheckRate(List<SData> accountList, SData param, ref string outacc)
        {
            var accList = new List<string>();
            foreach (var item in accountList)
            {
                decimal.TryParse(item["a_sum_arate"]?.ToString(), out decimal arate);
                if (arate != 100)
                {
                    throw new Exception(ApiTask.LEnd($"同一{param["exName"]}的转出余额比例不等于100%，不允许结转"));
                }

                // 获取所有转出科目，作为凭证分录查询条件
                var accId = item[param["keyName"].ToString()].ToString();
                outacc += $"{accId},";
                accList.Add(accId);
            }

            return accList;
        }

        /// <summary>
        /// 生成结转凭证
        /// </summary>
        /// <param name="voucherDList">原始凭证</param>
        /// <param name="inaccList">转入科目信息</param>
        /// <param name="data">要新增的结转凭证体数据</param>
        private void CreateVoucherD(List<SData> voucherDList, List<SData> inaccList, SData data, List<SData> yearEndAList)
        {
            // * 添加转出科目凭证分录
            var outaccAmountList = new Dictionary<int, decimal>();

            foreach (var voucherD in voucherDList)
            {
                int.TryParse(voucherD["account__id"]?.ToString(), out int accountId);
                int.TryParse(voucherD["account__flow"]?.ToString(), out int flow);
                var amount = voucherD["a_sum_amount"].ToString().ToDec();

                // 科目余额方向为贷时将结果乘以 - 1
                if (flow == AcConst.Credit)
                {
                    amount = amount * -1;
                }

                // 累计同一个转出科目的期末余额
                if (outaccAmountList.ContainsKey(accountId))
                {
                    outaccAmountList[accountId] += amount;
                }
                else
                {
                    outaccAmountList.Add(accountId, amount);
                }

                var accountData = new SData("amount", amount, "account", accountId, "flow", flow, "unitname", voucherD["account__unitname"]);

                // 添加凭证分录
                InsertYearEndVoucherD(data, accountData, voucherD);
            }

            // * 添加转入科目凭证分录
            if (inaccList?.Count > 0)
            {
                // 同一定义科目记录下转出科目金额（针对转出科目性质下有多个科目时）合并
                var inaccAmountDataList = new Dictionary<int, SData>();

                foreach (var inacc in inaccList)
                {
                    var outaccId = -1;
                    var outaccFlow = -1;
                    var property = inacc.Item<string>("p_property");
                    if (!string.IsNullOrEmpty(property) && property != "0")
                    {
                        int.TryParse(inacc["p_accountId"]?.ToString(), out outaccId);
                        int.TryParse(inacc["p_flow"]?.ToString(), out outaccFlow);
                    }
                    else
                    {
                        int.TryParse(inacc["outacc_id"]?.ToString(), out outaccId);
                        int.TryParse(inacc["outacc_flow"]?.ToString(), out outaccFlow);
                    }

                    // 转出科目没有需要结转的余额数据时，对应的转入科目也不添加数据
                    if (outaccAmountList.ContainsKey(outaccId))
                    {
                        int.TryParse(inacc["inacc_id"]?.ToString(), out int inaccId);
                        int.TryParse(inacc["inacc_flow"]?.ToString(), out int inaccFlow);
                        decimal.TryParse(inacc["a_sum_arate"]?.ToString(), out decimal arate);

                        var amount = (outaccAmountList[outaccId] * arate / 100).ToString(CultureInfo.InvariantCulture).ToDec();

                        // 转出科目和转入科目余额方向相同，转入科目金额乘以-1
                        if (inaccFlow == outaccFlow)
                        {
                            amount = amount * -1;
                        }

                        int.TryParse(inacc["yearEndAId"]?.ToString(), out int yearEndAId);
                        var accountData = new SData("amount", amount, "account", inaccId, "flow", inaccFlow, "unitname", inacc["inacc_unitname"], "currency", inacc["inacc_currency__id"]);

                        // 累计同一个定义科目记录的期末余额（针对转出科目性质下有多个科目时）
                        if (inaccAmountDataList.ContainsKey(yearEndAId))
                        {
                            inaccAmountDataList[yearEndAId]["amount"] = (inaccAmountDataList[yearEndAId]["amount"].ToString().ToDec() + amount).ToString(CultureInfo.InvariantCulture).ToDec();
                        }
                        else
                        {
                            inaccAmountDataList.Add(yearEndAId, accountData);
                        }
                    }
                }

                // 添加转出科目凭证(定义了几条转入科目，就有几条转入科目凭证)
                foreach (var inaccAmount in inaccAmountDataList)
                {
                    var yearEndA = yearEndAList.FirstOrDefault(x => x["id"].ToString() == inaccAmount.Key.ToString());
                    var currency = yearEndA.Item<string>("currency.id");
                    var accountData = inaccAmount.Value;
                    if (string.IsNullOrEmpty(accountData.Item<string>("currency")) && !string.IsNullOrEmpty(currency))
                    {
                        accountData["currency"] = currency;
                    }

                    // 添加凭证分录
                    InsertYearEndVoucherD(data, accountData, yearEndA);
                }
            }
        }

        /// <summary>
        /// 初始化结转凭证头
        /// </summary>
        /// <param name="data">凭证头实体</param>
        /// <param name="acYearEnd">自动凭证结转实体</param>
        private void InitYearEndVoucher(SData data, SData acYearEnd)
        {
            // todo 在类定义中，凭证头编号规则为手工编号，则按时间随机编号，凭证头编号规则为非手工编号，则根据编号规则编号
            data["dcode"] = Guid.NewGuid().ToString();

            data["title"] = "结转凭证头";

            // 凭证类型
            data["vtype"] = acYearEnd["vtype"];

            // 预算方案
            data["vclass"] = acYearEnd["vclass"];

            // 核算期
            data["period"] = acYearEnd["period"];

            // 状态为草稿
            data["vstate"] = AcConst.Draft;

            // 制单日期为当天日期
            data["mdate"] = DateTime.Now.ToString("yyyy-MM-dd");

            // 修改日期为当天日期
            data["udate"] = DateTime.Now.ToString("yyyy-MM-dd");

            // 制单人为当前登录用户
            data["maker"] = this.ApiTask.UserInfo().UserCode();

            // 摘要
            data["ddesc"] = acYearEnd["ddesc"];

            // 自动凭证结转id
            data["yearend"] = $"[{acYearEnd["id"]}]";

            // 业务日期
            data["bdate"] = DateTime.Now.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 新增自动凭证结转凭证分录
        /// </summary>
        /// <param name="data">自动凭证结转凭证头</param>
        /// <param name="accountData">科目余额信息</param>
        /// <param name="voucherD">凭证头信息</param>
        private void InsertYearEndVoucherD(SData data, SData accountData, SData voucherD = null)
        {
            var yearEndVd = new SData();
            yearEndVd["vh"] = $"[{data["id"]}]";

            if (accountData["flow"].Equals(AcConst.Debit))
            {
                // 科目余额方向在借，生成的凭证体金额放到贷
                yearEndVd["amountc"] = accountData["amount"];

                // 借贷标志，0借 1贷
                yearEndVd["cd"] = AcConst.Credit;
            }
            else
            {
                // 科目余额方向在贷，生成的凭证体金额放到借
                yearEndVd["amountd"] = accountData["amount"];

                // 借贷标志，0借 1贷
                yearEndVd["cd"] = AcConst.Debit;
            }

            // 核算单位为原始凭证的核算单位
            yearEndVd["subunit"] = data["subunit"];

            // 预算方案
            yearEndVd["vclass"] = data["vclass"];

            // 核算期
            yearEndVd["period"] = data["period"];

            // 业务日期
            yearEndVd["bdate"] = DateTime.Now.ToString("yyyy-MM-dd");

            // 业务参考
            yearEndVd["reference"] = "自动结转";

            // 摘要
            yearEndVd["ddesc"] = data["ddesc"];

            // 到期日期
            yearEndVd["edate"] = DateTime.Now.ToString("yyyy-MM-dd");

            // 科目
            yearEndVd["account"] = $"[{accountData["account"]}]";

            // 计量单位
            yearEndVd["unitname"] = accountData["unitname"];

            // 状态为草稿
            yearEndVd["vstate"] = AcConst.Draft;

            if (voucherD != null)
            {
                // tcode
                this.ApiTask.Tcode().ForEachKeys(t =>
                {
                    if (voucherD.ContainsKey(t))
                    {
                        yearEndVd[t] = $"[{voucherD[$"{t}.id"]}]";
                    }
                    else
                    {
                        yearEndVd[t] = $"[{voucherD[$"{t}__id"]}]";
                    }
                });
            }

            if (!string.IsNullOrEmpty(accountData.Item<string>("currency")))
            {
                yearEndVd["currency"] = $"[{accountData["currency"]}]";
            }

            // 添加结转凭证体
            var acYearEndVdBiz = ApiTask.Biz<BizTableVoucherD>(nameof(AcYearEndVd));
            acYearEndVdBiz.Insert(yearEndVd);
        }

        /// <summary>
        /// 修改凭证头贷方或者借方合计值
        /// </summary>
        /// <param name="voucherId">凭证头Id</param>
        private void UpdateVoucherSumdAndSumc(int voucherId)
        {
            var voucherDBiz = ApiTask.Biz<BizTableVoucherD>(nameof(AcYearEndVd));
            var voucherBiz = ApiTask.Biz<BizTableVoucher>(nameof(AcYearEndV));

            var parms = new SData("vh", $"[{voucherId}]").toParmStr();

            // 借方合计
            var amountdSum = voucherDBiz.GetAggregate(parms, EAggregate.Sum, "amountd").ToString().ToDec();

            // 贷方合计
            var amountcSum = voucherDBiz.GetAggregate(parms, EAggregate.Sum, "amountc").ToString().ToDec();

            // 修改凭证头借方合计和贷方合计
            voucherBiz.Update(new SData("id", voucherId, "sumd", amountdSum, "sumc", amountcSum));
        }

        /// <summary>
        /// 根据转出科目性质、转出科目、转入科目分组求结转余额比例和
        /// </summary>
        /// <param name="yearend">自动凭证结转</param>
        /// <param name="proAccList">（有转出科目性质）转出科目集合</param>
        /// <param name="outAccList">（无转出科目性质）转出科目集合</param>
        /// <param name="proInAccList">（有转出科目性质）转入科目集合</param>
        /// <param name="inAccList">（无转出科目性质）转入科目集合</param>
        private void GetAccountList(object yearend, ref List<SData> proAccList, ref List<SData> outAccList, ref List<SData> proInAccList, ref List<SData> inAccList)
        {
            var acYearEndABiz = ApiTask.Biz<BizTableCode>(nameof(AcYearEndA));

            // 根据转出科目性质、转出科目、转入科目分组求结转余额比例和
            var sql = $@"select 
acc.id As p_accountid,
acc.flow As p_flow,
m.property As p_property,
sum( m.d0 ) As a_sum_arate,
m_outacc.id As outacc_id,
m_outacc.flow As outacc_flow,
m_inacc.id As inacc_id,
m_inacc.flow As inacc_flow,
m_inacc.unitname As inacc_unitname,
m_inacc_currency.id As inacc_currency_id,
m.id As yearendaid 
from 
account as m 
left join account as acc on acc.property = m.property and acc.dclass = 'a' 
left join account as m_outacc on m.a19 = m_outacc.id and m_outacc.dclass = 'a'  
left join account as m_inacc on m.a18 = m_inacc.id and m_inacc.dclass = 'a' 
left join others as m_inacc_currency on m_inacc.currency = m_inacc_currency.id and m_inacc_currency.dclass = 'accurrency' 
where 
m.dclass = 'acyearenda' And 
m.a17 = {yearend} 
group by 
m.property,
acc.id,
acc.flow,
m_outacc.id,
m_outacc.flow,
m_inacc.id,
m_inacc.flow,
m_inacc.unitname,
m_inacc_currency.id,
m.id";

            var list = acYearEndABiz.ApiTask.DB.ExecuteList(sql);

            // 自动凭证结转没有定义转出科目，提示：请先定义需要结转的转出科目
            if (list.Count == 0)
            {
                throw new Exception(ApiTask.LEnd("请先定义需要结转的转出科目"));
            }

            // 按照(有转出科目性质)转出科目分组获取比例合计
            proAccList = list.Where(x => x["p_property"] != null && !x["p_property"].Equals(0) && x["p_accountId"] != null).GroupBy(x => x["p_accountId"]).Select(group =>
            {
                var dic = new SData("p_accountId", group.Key, "a_sum_arate", group.Sum(a => a["a_sum_arate"].ToString().ToDec()));
                return dic;
            }).ToList();

            // 按照(无转出科目性质)转出科目分组获取比例合计
            outAccList = list.Where(x => x["p_property"] == null || x["p_property"].Equals(0)).GroupBy(x => x["outacc_id"]).Select(group =>
                   {
                       var dic = new SData("outacc_id", group.Key, "a_sum_arate", group.Sum(a => a["a_sum_arate"].ToString().ToDec()));
                       return dic;
                   }).ToList();

            // 筛选出转入科目不为空的集合
            // (有转出科目性质)转入科目
            proInAccList = list.Where(x => x["p_property"] != null && !x["p_property"].Equals(0) && x["p_accountId"] != null && x["inacc_id"] != null).ToList();

            // (无转出科目性质)转入科目
            inAccList = list.Where(x => (x["p_property"] == null || x["p_property"].Equals(0)) && x["inacc_id"] != null).ToList();
        }

        /// <summary>
        /// 根据科目、Tcode统计凭证分录的[期末余额]
        /// </summary>
        /// <param name="data">自动凭证结转实体</param>
        /// <param name="outacc">转出科目</param>
        /// <param name="subunitStr">已经结转的核算单位</param>
        /// <returns>科目、Tcode分组集合</returns>
        private List<SData> GetVoucherDList(SData data, string outacc, string subunitStr)
        {
            // todo outacc科目很多时，sql报错？
            var acVoucherDBiz = ApiTask.Biz<BizTableVoucherD>(nameof(AcVoucherD));
            var vstateId = ConstResourceDB.GetId(this.ApiTask, nameof(AcVstate), AcConst.Trial);
            var queryParms = new SData("vclass", $"[{data["vclass.id"]}]", "period", $":{data["period"]}", "account.id", outacc, "vstate", $"[{vstateId}]", "subunit.id", subunitStr);

            var queryFields = new List<string>()
            {
                "account.flow",
                "account.unitname",
                "a_sum_amount",
            };
            var groupFields = new List<string>()
            {
                "subunit.id",
                "account.id",
                "account.flow",
                "account.unitname",
            };

            // Tcode，将类定义中启用的tcode添加到分组数据中
            this.ApiTask.Tcode().ForEachKeys(t =>
            {
                groupFields.Add($"{t}.id");
            });

            // 查询数据
            var sql = acVoucherDBiz.QuerySql(queryParms, queryFields, groupFields, null, false);
            Env.Log($"获取凭证期末余额sql：{sql}");
            var voucherDList = acVoucherDBiz.ApiTask.DB.ExecuteList(sql);

            // 无凭证分录数据，提示：转出科目没有需要结转的余额数据
            if (voucherDList.Count == 0)
            {
                throw new Exception(ApiTask.LEnd("转出科目没有需要结转的余额数据"));
            }

            // 期末余额都为0，提示：转出科目没有需要结转的余额数据

            // 按照核算单位、转出科目分组获取期末余额合计
            var sumAmountGroupBySubunitOutAcc = voucherDList.GroupBy(x => $"{x["account__id"]}.{x["subunit__id"]}").Select(group =>
            {
                var dic = new SData("account__id", group.Key.Sp_First(), "subunit__id", group.Key.Sp_Last(), "a_sum_amount", group.Sum(a => a["a_sum_amount"].ToString().ToDec()));
                return dic;
            }).ToList();

            // 所有核算单位的转出科目期末余额不为0的集合
            var sumAmountGroupBySubunit = sumAmountGroupBySubunitOutAcc.Where(x => x["a_sum_amount"].ToString().ToDec() != 0).ToList();
            if (sumAmountGroupBySubunit.Count == 0)
            {
                throw new Exception(ApiTask.LEnd("转出科目没有需要结转的余额数据"));
            }

            // 筛选出转出科目的期末余额不为0的转出科目
            var accList = new List<string>();
            foreach (var acc in sumAmountGroupBySubunit)
            {
                accList.Add(acc["account__id"].ToString());
            }

            // 筛选出（科目、Tcode、币种）分组后期末余额不为0的数据
            var amountNotZeroList = voucherDList.Where(x => x["a_sum_amount"].ToString().ToDec() != 0 && accList.Contains(x["account__id"].ToString())).ToList();
            return amountNotZeroList;
        }
        #endregion

    }
}
