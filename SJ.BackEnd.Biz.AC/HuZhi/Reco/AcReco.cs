#region << 版 本 注 释 >>
/* ==============================================================================
// <copyright file="ACReco.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：勾对头
* 创 建 人：胡智
* 创建日期：2019-09-26 15:49:20
* ==============================================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 勾对头
    /// 1.描述：自动勾对
    /// 2.约定
    ///     1.GetList方法中约定一个键type：z3(自动勾对)
    /// 3.业务逻辑
    ///     1.查询前：自动勾对添加id=-1的参数、登录用户的核算单位不为空时添加核算单位条件
    ///     2.查询后：自动勾对：设置核算单位查询条件、获取符合查询条件的借方凭证分录数据、筛选出(科目相同、金额相同、分析项且分析项值相同)的贷方数据进行勾对
    /// </summary>
    [ClassData("cname", "勾对头", "vision", 1)]
    public class AcReco : BizTableVoucher
    {
        #region 框架方法重写

        /// <summary>
        /// 查询前逻辑
        /// 1.自动勾对,添加id=-1参数
        /// 2.登录用户的核算单位不为空时添加核算单位条件
        /// 3.设置处理日期条件、核算期的区间
        /// </summary>
        /// <param name="qParm">查询参数</param>
        protected override void OnGetListBefore(SData qParm)
        {
            // 数据准备 **********
            // 变量声明
            string type = qParm.Item<string>("type");

            // 参数定义 **********
            // * 自动勾对,添加id=-1参数
            if (AcConst.RecoAuto.Equals(type))
            {
                qParm.Add("id", -1);
                return;
            }

            // * 设置处理日期条件、核算期的区间
            qParm.ConvertDate("bdate");
            qParm.ConvertDate("period");
            base.OnGetListBefore(qParm);
        }

        /// <summary>
        /// 查询后逻辑
        /// 1.自动勾对，返回提示信息:(自动勾对完成,成功勾对XX条数据,勾对金额XX元)
        /// </summary>
        /// <param name="qParm">查询参数</param>
        /// <param name="Data">返回数据</param>
        protected override void OnGetListAfter(SData qParm, List<SData> Data)
        {
            // 数据准备 **********
            // 变量声明
            string type = qParm.Item<string>("type");

            // 业务逻辑 **********
            // 自动勾对
            if (AcConst.RecoAuto.Equals(type))
            {
                #region 必填验证

                // 核算单位必填
                if (string.IsNullOrEmpty(qParm.Item<string>("subunits")))
                {
                    throw new Exception($"{ApiTask.L("核算单位")}{ApiTask.LEnd(PubLang.NotEmpty)}");
                }

                // 验证科目对账必填
                if (string.IsNullOrEmpty(qParm.Item<string>("subrec")))
                {
                    throw new Exception($"{ApiTask.L("科目对账")}{ApiTask.LEnd(PubLang.NotEmpty)}");
                }

                // 验证勾对规则必填
                if (string.IsNullOrEmpty(qParm.Item<string>("rule")))
                {
                    throw new Exception($"{ApiTask.L("勾对规则")}{ApiTask.LEnd(PubLang.NotEmpty)}");
                }
                #endregion

                // 科目对账Code
                string subjectRecCode = qParm["subrec"].ToString().Sp_First(",").Sp_First(".");
                SData subjectRec = ApiTask.Biz<BizTable>("AcSubRec").GetItemByParms(new SData("dcode", subjectRecCode).toParmStr()); // 获取科目对账
                if (subjectRec == null)
                {
                    throw new Exception($"{ApiTask.L("科目对账")}{ApiTask.LEnd(PubLang.NotExists)}");
                }

                // * 根据条件获取科目(如果选了科目,判断科目是否在所选科目对账中，如果没选科目，查询科目对账中所有的科目)
                List<string> subjectList = GetSubjects(qParm, subjectRecCode, ApiTask);
                if (subjectList.Count == 0)
                {
                    throw new Exception($"{ApiTask.L("科目")}{ApiTask.LEnd(PubLang.NotExists)}");
                }

                // 设置核算单位
                SetSubUnit(qParm);

                string message = AutoReco(qParm, subjectList, subjectRec);
                Data.Add(new SData("message", message));
            }

            base.OnGetListAfter(qParm, Data);
        }

        /// <summary>
        /// 屏蔽基类方法，避免占用凭证编号
        /// </summary>
        /// <param name="data">勾对头</param>
        protected override void OnInsertBefore(SData data)
        {

        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 自动勾对
        /// 1.变量声明
        /// 2.获取过滤条件
        /// 3.获取所有借方数据按业务日期分组
        /// 4.根据筛选条件获取符合要求的凭证分录数据(借方的数据)
        /// 5.遍历金额在借方的数据
        /// 6.筛选出(科目相同 、金额相同、分析项且分析项值相同)的贷方数据
        /// 7.添加勾对体
        /// </summary>
        /// <param name="data">查询参数</param>
        /// <param name="subjectList">科目id集合</param>
        /// <param name="subjectRec">科目对账实体</param>
        /// <returns>勾对完成消息</returns>
        private string AutoReco(SData data, List<string> subjectList, SData subjectRec)
        {
            Stopwatch autowatch = new Stopwatch();
            autowatch.Restart();
            Env.Log($"开始自动勾对..,时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");

            // 数据准备 **********
            // *变量声明
            var acRecoDBiz = ApiTask.Biz<BizTable>("AcRecoD");
            var acVoucherDRecoBiz = ApiTask.Biz<BizTable>("AcVoucherDReco");
            int recoCount = 0; // 记录勾对体成功添加的条数
            decimal recoAmountSum = 0; // 总的勾对金额
            SData voucherCredit = null; // 金额在贷方的凭证
            SData queryCondition = null; // 查询条件
            int voucherdCount = 0; // 符合要求的凭证分录数
            string maxbDate = string.IsNullOrEmpty(data.Item<string>("bdateb")) ? "1900-01-01" : data.Item<string>("bdateb"); // 保存每次获取记录的最大日期
            int voucherdIdMax = 0; // 保存每次获取记录最大凭证Id

            string loginUser = ApiTask.UserInfo().UserCode();
            int subjectRecId = int.Parse(subjectRec["id"].ToString());

            // 获取科目对账中启用的分析项
            List<string> tcodeList = GetTcodes(subjectRec);

            // 业务逻辑 **********
            data.Append("bdateb", $"{maxbDate}");

            // * 获取过滤条件
            SData filterParamDebit = GetFilterParameters(data, subjectList, subjectRecId); // 借方查询条件
            filterParamDebit.Append("cd", AcConst.Debit);

            SData filterParamCredit = GetFilterParameters(data, subjectList, subjectRecId); // 贷方查询条件
            filterParamCredit.Append("cd", AcConst.Credit);

            Env.Log($"开始..业务日期分组,时间:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            Stopwatch groupwatch = new Stopwatch();
            groupwatch.Restart();

            // * 获取所有借方数据按业务日期分组
            var bdateList = GetVoucherD(data, filterParamDebit, tcodeList, 0);
            if (bdateList.Count == 0)
            {
                throw new Exception(ApiTask.LEnd("无未勾对的数据"));
            }

            groupwatch.Stop();
            Env.Log($"结束..业务日期分组,用时:{groupwatch.ElapsedMilliseconds}毫秒");

            foreach (var bdate in bdateList)
            {
                voucherdIdMax = 0;
                maxbDate = bdate["bdate"].ToString();

                do
                {
                    try
                    {
                        data.Append("bdate", $"{maxbDate}");
                        data.Append("voucherdid", $"!:{voucherdIdMax}");

                        // * 根据筛选条件获取符合要求的凭证分录数据(借方的数据)
                        List<SData> voucherDebitList = GetVoucherD(data, filterParamDebit, tcodeList, AcConst.RecoPageSize);
                        voucherdCount = voucherDebitList.Count;

                        if (voucherdCount > 0)
                        {
                            // 获取最后一条凭证，记录最大id
                            var lastVoucherdItem = voucherDebitList.LastOrDefault();
                            voucherdIdMax = lastVoucherdItem.Item<int>("id");
                        }

                        // * 遍历金额在借方的数据
                        foreach (var voucherDebit in voucherDebitList)
                        {
                            // 查询条件(科目相同、金额相同)
                            queryCondition = filterParamCredit;
                            queryCondition.Append("account.id", voucherDebit["account.id"]);
                            queryCondition.Append("amountc", "[" + voucherDebit["amountd"] + "]");

                            // 添加科目对账中设置的Tcode条件 Tcode不能为空
                            foreach (var tcode in tcodeList)
                            {
                                queryCondition.Append(tcode, voucherDebit[tcode]?.ToString());
                            }

                            var voucherCreditList = acVoucherDRecoBiz.GetListData(1, 0, queryCondition.toParmStr(), "id,cd,amountc,SubUnit,vclass,vstate");

                            // * 筛选出(科目相同 、金额相同、分析项且分析项值相同)的贷方数据
                            voucherCredit = voucherCreditList.FirstOrDefault();

                            if (voucherCredit != null)
                            {
                                try
                                {
                                    // * 添加勾对体
                                    // 批次
                                    string batch = AcVoucherHelper.GetRecoBatch(this.ApiTask);

                                    // 给勾对体数据赋值
                                    SData recoDData = new SData("rstate", AcConst.AllTick, "batch", batch, "recoperson", loginUser, "recoamount", voucherDebit["amountd"], AcConst.RecoTcode, $"[{subjectRecId}]", "cd", voucherDebit["cd"], "logicsign", AcConst.RecoAuto, "SubUnit", voucherDebit["SubUnit"], "parent", voucherDebit["id"]);

                                    List<SData> list = new List<SData>()
                                    {
                                       new SData("rstate", AcConst.AllTick, "batch", batch, "recoperson", loginUser, "recoamount", voucherCredit["amountc"], AcConst.RecoTcode, $"[{subjectRecId}]", "cd", voucherCredit["cd"], "logicsign", AcConst.RecoAuto, "SubUnit", voucherCredit["SubUnit"], "parent", voucherCredit["id"]),
                                    };
                                    recoDData.Append("recod", list);

                                    int recoDId = acRecoDBiz.Insert(recoDData);
                                    if (recoDId > 0)
                                    {
                                        recoCount += 2;
                                        recoAmountSum += voucherDebit["amountd"].ToString().ToDec();
                                    }
                                }
                                catch (Exception e)
                                {
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                while (voucherdCount == AcConst.RecoPageSize);
            }

            if (recoCount == 0)
            {
                // 遍历完成更新勾对头信息
                throw new Exception(ApiTask.LEnd("无未勾对的数据"));
            }

            autowatch.Stop();

            Env.Log($"结束自动勾对[{recoCount}]用时：{autowatch.ElapsedMilliseconds}毫秒");

            return $"自动勾对完成，勾对{recoCount}条数据，勾对金额{recoAmountSum}元";
        }

        /// <summary>
        /// 设置核算单位查询条件
        /// 1.登录用户的核算单位不等于条件传入的核算单位(登录用户的核算单位不为空、条件传入核算单位不为空)
        /// </summary>
        /// <param name="data">勾对体数据</param>
        private void SetSubUnit(SData data)
        {
            // 数据准备 **********
            // 当前核算单位
            string currentSubunit = ApiTask.UserInfo().UserSubUnit();

            // 业务逻辑 **********
            // 条件传入核算单位不为空、登录用户的核算单位不为空
            if (!string.IsNullOrEmpty(currentSubunit))
            {
                // * 登录用户的核算单位不等于条件传入的核算单位、登录用户的核算单位不为空、条件参数传入核算单位不为空(提示核算单位不一致)
                if (!ApiTask.UserInfo().UserSubUnit().Sp_First(".").Equals(data.Item<string>("subunits").Sp_First(",").Sp_First(".")))
                {
                    throw new Exception(ApiTask.LEnd("核算单位不一致，不允许勾对"));
                }
            }
        }

        /// <summary>
        /// 根据科目对账获取科目对账设置的Tcode
        /// 1.获取科目对账
        /// 2.找到启用的Tcode
        /// </summary>
        /// <param name="subjectRec">科目对账</param>
        /// <returns>返回科目对账设置的Tcode</returns>
        private List<string> GetTcodes(SData subjectRec)
        {
            // 数据准备 **********
            // 变量声明
            List<string> tCodeList = new List<string>(); // 保存科目对账设置的Tcode

            // 业务逻辑 **********
            // * 找到启用的Tcode
            var tcodeValueList = subjectRec.Where(x => Regex.IsMatch(x.Key, Regx.TcodeModel) && x.Value.Equals(true));

            if (tcodeValueList.Any())
            {
                foreach (var item in tcodeValueList)
                {
                    tCodeList.Add(item.Key);
                }
            }

            return tCodeList;
        }

        /// <summary>
        /// 根据科目和科目对账Code得到科目对账与科目关系
        /// </summary>
        /// <param name="queryParam">自动勾对查询条件</param>
        /// <param name="subjectRecCode">科目对账code</param>
        /// <param name="task">上下文对象</param>
        /// <returns>返回满足条件的科目，没有返回空</returns>
        private List<string> GetSubjects(SData queryParam, string subjectRecCode, ApiTask task)
        {
            // * 根据科目和科目对账Code得到科目对账与科目关系
            var account = queryParam.Item<string>("subject");
            List<SData> reconRefData = task.Biz<BizTable>("AcReconRef").GetListData(0, 0, new SData("subrec", subjectRecCode, "account", account).toParmStr(), "id,subrec,account,isreverse");

            if (string.IsNullOrEmpty(account))
            {
                if (reconRefData.Count == 0)
                {
                    throw new Exception(ApiTask.LEnd("科目对账没设置科目"));
                }
            }
            else
            {
                if (reconRefData.Count == 0)
                {
                    throw new Exception(ApiTask.LEnd("科目不在科目对账中"));
                }
            }

            // 存科目Id
            List<string> subjectList = new List<string>();

            // 获取科目对账中设置的科目
            foreach (var reconRef in reconRefData)
            {
                subjectList.Add(reconRef["account.id"].ToString());
            }

            return subjectList;
        }

        /// <summary>
        /// 查询出满足条件的凭证分录
        /// </summary>
        /// <param name="queryParam">更新后的查询条件</param>
        /// <param name="filterParameters">满足条件的科目</param>
        /// <param name="tcodeList">科目对账启用的分析项</param>
        /// <param name="pageSize">页码</param>
        /// <returns>满足条件的凭证分录</returns>
        private List<SData> GetVoucherD(SData queryParam, SData filterParameters, List<string> tcodeList, int pageSize)
        {
            string paramFormat = "{0}:{1}"; // 合成参数格式 (前端请求的periodb[201910]和periode[201911]，合成period[201910:201911])

            // 添加业务日期条件
            if (string.IsNullOrEmpty(queryParam.Item<string>("bdate")))
            {
                string bdate = string.Format(paramFormat, queryParam.Item<string>("bdateb"), queryParam.Item<string>("bdatee"));
                if (!bdate.Equals(":"))
                {
                    filterParameters.Append("bdate", bdate);
                }
            }
            else
            {
                filterParameters.Append("bdate", queryParam.Item<string>("bdate"));
            }

            // 排除勾对过的凭证分录
            if (!string.IsNullOrEmpty(queryParam.Item<string>("voucherdid")))
            {
                filterParameters.Append("id", queryParam["voucherdid"].ToString());
            }

            var voucherDData = new List<SData>();
            var biz = ApiTask.Biz<BizTable>("AcVoucherDReco");

            if (pageSize > 0)
            {
                // 凭证分录返回的列
                string columns = "id,cd,bdate,account.id,amountc,amountd,SubUnit,vstate,vclass";
                string tcodes = string.Join(",", tcodeList);
                if (!string.IsNullOrEmpty(tcodes))
                {
                    columns += "," + tcodes;
                }

                // * 获取凭证分录数据
                voucherDData = biz.GetListData(pageSize, 0, filterParameters.toParmStr(), columns);
            }
            else
            {
                // 获取凭证的勾对金额
                var queryFields = new List<string>() { "bdate" };
                var groupFields = new List<string>() { "bdate" };
                var sql = biz.QuerySql(filterParameters, queryFields, groupFields, null, false);
                voucherDData = biz.ApiTask.DB.ExecuteList(sql);

                Env.Log($"借方业务日期分组sql：{sql}记录数:{voucherDData.Count}");
            }

            return voucherDData;
        }

        /// <summary>
        /// 获取自动勾对查询条件
        /// </summary>
        /// <param name="queryParam">查询条件</param>
        /// <param name="subjectList">满足条件的科目</param>
        /// <param name="subjectRecId">科目对账Id</param>
        /// <returns>自动勾对查询条件</returns>
        private SData GetFilterParameters(SData queryParam, List<string> subjectList, int subjectRecId)
        {
            SData filterParameters = new SData(); // 过滤条件
            string paramFormat = "{0}:{1}"; // 合成参数格式 (前端请求的periodb[201910]和periode[201911]，合成period[201910:201911])

            // 凭证编号有值
            if (!string.IsNullOrEmpty(queryParam.Item<string>("voucherno")))
            {
                filterParameters.Add("vh.dcode", queryParam["voucherno"].ToString());
            }

            // 凭证类型有值
            if (!string.IsNullOrEmpty(queryParam.Item<string>("vouchertype")))
            {
                filterParameters.Add("vh.vtype", queryParam["vouchertype"].ToString());
            }

            // 核算期为空或者为0时赋空值
            if (string.IsNullOrEmpty(queryParam.Item<string>("periodb")) || queryParam.Item<string>("periodb").Equals("0"))
            {
                queryParam.Append("periodb", AcConst.PeriodBegin);
            }

            if (string.IsNullOrEmpty(queryParam.Item<string>("periode")) || queryParam.Item<string>("periode").Equals("0"))
            {
                queryParam.Append("periode", AcConst.PeriodEnd);
            }

            // 添加核算期条件
            string period = string.Format(paramFormat, queryParam.Item<string>("periodb"), queryParam.Item<string>("periode"));
            if (!period.Equals(":"))
            {
                filterParameters.Append("period", period);
            }

            // 添加业务参考条件
            if (!string.IsNullOrEmpty(queryParam.Item<string>("reference")))
            {
                filterParameters.Append("reference", queryParam["reference"].ToString());
            }

            // 添加币种条件
            if (!string.IsNullOrEmpty(queryParam.Item<string>("currency")))
            {
                filterParameters.Append("currency", queryParam["currency"].ToString());
            }

            // 添加核算单位
            if (!string.IsNullOrEmpty(queryParam.Item<string>("subunits")))
            {
                filterParameters.Append("subunit", queryParam["subunits"].ToString().Sp_First(","));
                //var subunitId = ConstResourceDB.GetId(nameof(AcSubunit), queryParam["subunits"].ToString().Sp_First(","));
                //filterParameters.Append("subunit", $"[{subunitId}]");
            }

            // 添加摘要条件
            if (!string.IsNullOrEmpty(queryParam.Item<string>("ddesc")))
            {
                filterParameters.Append("ddesc", queryParam["ddesc"].ToString());
            }

            // 隐含条件,实际数据,记账状态,
            var vclassId = ConstResourceDB.GetId(this.ApiTask, nameof(AcVclass), AcConst.ActData);
            filterParameters.Append("vclass", $"[{vclassId}]");

            var vstateId = ConstResourceDB.GetId(this.ApiTask, nameof(AcVstate), AcConst.Trial);
            filterParameters.Append("vstate", $"[{vstateId}]");

            filterParameters.Append("q.orderby", "bdate,id");

            // 添加科目条件
            if (subjectList.Count > 0)
            {
                string[] subjects = subjectList.ToArray();
                filterParameters.Append("account.id", string.Join(",", subjects));
            }

            // 自定义查询条件 从未勾对的数据
            filterParameters.Append("autoreco", subjectRecId);

            // 从参数中找到对应的Tcode字段名和Tcode值(传入格式"t1=a.code,t2=")
            foreach (var item in queryParam)
            {
                if (Regex.IsMatch(item.Key, Regx.TcodeModel) && !string.IsNullOrEmpty(item.Value.ToString()))
                {
                    filterParameters.Append(item.Key, item.Value);
                }
            }

            return filterParameters;
        }

        #endregion
    }
}
