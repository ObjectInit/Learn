#region  << 版 本 注 释 >>
/* ============================================================================== 
// <copyright file="AcVoucherHelper.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcVoucherHelper 
* 创 建 者：龚绍平
* 创建时间：2019/11/13 16:20:09 
* ==============================================================================*/
#endregion

using System;
using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 凭证辅助类
    /// 1 描述：凭证单据的基本辅助方法
    /// 2 约定: 无
    /// 3 业务逻辑：
    ///     1.获取Tcode
    ///     2.根据科目对账获取科目对账设置的分析项
    ///     3.检查凭证分录的分析项是否相同
    ///     4.检查凭证分录的核算单位是否相同
    ///     5.查询得到付款单/收款单的待审金额
    ///     6.根据科目对账字符串得到科目对账的id,没找到则返回-1
    /// </summary>
    public static class AcVoucherHelper
    {
        /// <summary>
        /// Initializes static members of the <see cref="AcVoucherHelper"/> class.
        /// 构造函数
        /// </summary>
        static AcVoucherHelper()
        {
            var i = 0;
            while (i < 30)
            {
                // tcode串
                TcodeStr += $"t{i},";
                TcodeInfStr += $"inft{i},";
                TcodeNofStr += $"noft{i},";

                // 预留字段集合串
                TASDCodeStr += $"t{i},";

                if (i <= 9)
                {
                    TASDCodeStr += $"a{i},s{i},";
                }

                if (i <= 5)
                {
                    TASDCodeStr += $"d{i},";
                }

                i++;
            }

            TcodeStr = TcodeStr.TrimEnd(',');
            TASDCodeStr = TASDCodeStr.TrimEnd(',');
        }

        /// <summary>
        /// Tcode串：t0,t1,t2……,t29
        /// </summary>
        public static string TcodeStr { get; private set; }

        /// <summary>
        /// t0~t29、a0~a9、s0~s9、d0~d5
        /// </summary>
        public static string TASDCodeStr { get; private set; }

        /// <summary>
        /// tcode指定范围串：inft0,inft1,inft2……,inft29
        /// </summary>
        public static string TcodeInfStr { get; private set; }

        /// <summary>
        /// tcode禁用范围串：noft0,noft1,noft2……,noft29
        /// </summary>
        public static string TcodeNofStr { get; private set; }

        #region 往来对账

        private static Dictionary<string, Dictionary<string, long>> voucherNumDic = new Dictionary<string, Dictionary<string, long>>();
        private static Dictionary<string, Dictionary<string, long>> recoNumDic = new Dictionary<string, Dictionary<string, long>>();
        readonly static object _Sync = new object();
        /// <summary>
        /// 根据科目对账获取科目对账设置的分析项
        /// </summary>
        /// <param name="subjectRec">科目对账Code</param>
        /// <param name="apiTask">访问api</param>
        /// <returns>启用的分析项</returns>
        public static List<string> GetRecTcodes(string subjectRec, ApiTask apiTask)
        {
            // 数据准备 **********
            var tCodeList = new List<string>();

            // 业务逻辑 **********
            // 查询得到科目对账
            var acSubRecTable = apiTask.BizTableCode("AcSubRec");
            var subrecId = acSubRecTable.GetIDAuto(subjectRec);
            var acSubRec = acSubRecTable.GetItemByParms(new SData("id", subrecId).toParmStr(), "id," + TcodeStr);

            if (acSubRec == null)
            {
                return tCodeList;
            }

            // 循环查找到对应属性，找到启用的Tcode
            var matchTCode = Regx.TcodeModel;
            var props = acSubRec.Where(x => Regex.IsMatch(x.Key, matchTCode));

            // 将启用的Tcode查询出来
            var acDefTcodeList = apiTask.Biz<AcDefTcode>(nameof(AcDefTcode))
                .GetListData(0, 1, new SData("isenable", true).toParmStr(), "dcode");
            if (acDefTcodeList.Count == 0)
            {
                return tCodeList;
            }

            foreach (var prop in props)
            {
                if (string.IsNullOrEmpty(prop.Key) || prop.Value == null)
                {
                    continue;
                }

                var isEnable = prop.Value.ToString().ToLower() == "true";

                // 判断Tcode启用
                if (isEnable && acDefTcodeList.Exists(m => m.ContainsValue(prop.Key.Replace('t', 'z'))))
                {
                    tCodeList.Add(prop.Key);
                }
            }

            return tCodeList;
        }

        /// <summary>
        /// 检查凭证分录的核算单位是否相同
        /// </summary>
        /// <param name="voucherDList">凭证分录</param>
        /// <returns></returns>
        public static bool CheckSubUnitIsSame(List<SData> voucherDList)
        {
            if (string.IsNullOrEmpty(voucherDList.FirstOrDefault().Item<string>("SubUnit")))
            {
                return false;
            }

            return voucherDList.GroupBy(m => m["SubUnit"]).Count() <= 1;
        }

        /// <summary>
        /// 根据科目对账字符串得到科目对账的id,没找到则返回-1
        /// </summary>
        /// <param name="subjectRec">科目对账的id或者dcode</param>
        /// <param name="apiTask">访问对象</param>
        /// <returns></returns>
        public static string GetSubRecId(string subjectRec, ApiTask apiTask)
        {
            // 查询得到科目对账
            var acSubRecTable = apiTask.BizTableCode("AcSubRec");
            var subrecId = acSubRecTable.GetIDAuto(subjectRec);
            return subrecId.ToString();
        }

        /// <summary>
        /// 获得编号 PayReceipt
        /// 1、缓存存在key，则直接取值+1
        /// 2、缓存不存在key，则查询数据库，再放入缓存
        /// </summary>
        /// <param name="bizName">实体名</param>
        /// <returns>返回格式：yyMMdd000001</returns>
        public static string GetPayReceiptNumber(ApiTask task)
        {
            // 业务逻辑 **********
            // * 缓存存在key，则直接取值
            var numKey = $"{task.Domain}_payreceipt";
            var dayKey = DateTime.Today.ToString("yyMMdd");
            lock (_Sync)
            {
                if (recoNumDic.ContainsKey(numKey))
                {
                    if (recoNumDic[numKey].ContainsKey(dayKey))
                    {
                        recoNumDic[numKey][dayKey] = recoNumDic[numKey][dayKey] + 1;
                    }
                    else
                    {
                        recoNumDic[numKey].Add(dayKey, 0);
                        recoNumDic[numKey][dayKey] = GetMaxVoucherNumToday(task);
                        // 删除旧的key记录
                        var loseKey = recoNumDic[numKey].Keys.ToArray();
                        foreach (var t in loseKey)
                        {
                            if (t != dayKey)
                            {
                                recoNumDic[numKey].Remove(t);
                            }
                        }
                    }
                }
                else
                {
                    recoNumDic.Add(numKey, new Dictionary<string, long>());
                    recoNumDic[numKey].Add(dayKey, 0);
                    recoNumDic[numKey][dayKey] = GetMaxVoucherNumToday(task);
                }

                Env.Log($"产生单据号>>>{recoNumDic[numKey][dayKey]}");
                return recoNumDic[numKey][dayKey].ToString();
            }

        }

        public static void RemovePayReceiptNumber(ApiTask task)
        {
            var numKey = $"{task.Domain}_payreceipt";
            var dayKey = DateTime.Today.ToString("yyMMdd");
            if (recoNumDic.ContainsKey(numKey) && recoNumDic[numKey].ContainsKey(dayKey))
            {
                recoNumDic[numKey].Remove(dayKey);
            }
        }

        /// <summary>
        /// 获取勾对批次号
        /// </summary>
        /// <returns></returns>
        public static string GetRecoBatch(ApiTask task)
        {
            var numKey = $"{task.Domain}_recobatch";
            var dayKey = DateTime.Today.ToString("yyMMdd");
            lock (_Sync) // 防止并发，进行锁操作
            {
                if (recoNumDic.ContainsKey(numKey))
                {
                    if (recoNumDic[numKey].ContainsKey(dayKey))
                    {
                        recoNumDic[numKey][dayKey] = recoNumDic[numKey][dayKey] + 1;
                    }
                    else
                    {
                        recoNumDic[numKey].Add(dayKey, 0);
                        recoNumDic[numKey][dayKey] = GetMaxNumToday(task, nameof(AcRecoD), new SData("account", AcConst.RecoAccount, "bdate", DateTime.Today.ToStr()), "batch");
                        // 删除旧的key记录
                        var loseKey = recoNumDic[numKey].Keys.ToArray();
                        foreach (var t in loseKey)
                        {
                            if (t != dayKey)
                            {
                                recoNumDic[numKey].Remove(t);
                            }
                        }
                    }
                }
                else
                {
                    recoNumDic.Add(numKey, new Dictionary<string, long>());
                    recoNumDic[numKey].Add(dayKey, 0);
                    recoNumDic[numKey][dayKey] = GetMaxNumToday(task, nameof(AcRecoD), new SData("account", AcConst.RecoAccount, "bdate", DateTime.Today.ToStr()), "batch");
                }
                Env.Log($"产生批次>>>{recoNumDic[numKey][dayKey]}");
                return recoNumDic[numKey][dayKey].ToString();
            }
        }

        public static void RemoveBatch(ApiTask task)
        {
            var numKey = $"{task.Domain}_recobatch";
            var dayKey = DateTime.Today.ToString("yyMMdd");
            if (recoNumDic.ContainsKey(numKey) && recoNumDic[numKey].ContainsKey(dayKey))
            {
                recoNumDic[numKey].Remove(dayKey);
            }
        }

        /// <summary>
        /// 获取单据头最大的编号
        /// </summary>
        /// <param name="task">工作task</param>
        /// <param name="bizName">单据体名称</param>
        /// <param name="condition">条件</param>
        /// <param name="fieldName">编号属性</param>
        /// <returns></returns>
        private static long GetMaxVoucherNumToday(ApiTask task)
        {
            var maxDecode = task.DB.ExecuteScalar($"select top 1 dcode from voucher where (dclass='acpay' or dclass='acreceipt') and mdate='{DateTime.Today:yyyy-MM-dd}' order by id desc");
            long maxNum = 0;
            if (maxDecode != null)
            {
                try
                {
                    maxNum = long.Parse(maxDecode.ToString()) + 1;
                }
                catch (Exception)
                {
                    maxNum = long.Parse(System.DateTime.Today.ToString("yyMMdd") + "1".PadLeft(6, '0'));
                }
            }
            else
            {
                maxNum = long.Parse(System.DateTime.Today.ToString("yyMMdd") + "1".PadLeft(6, '0'));
            }

            return maxNum;
        }

        /// <summary>
        /// 获取单据体最大的编号
        /// </summary>
        /// <param name="task">工作task</param>
        /// <param name="bizName">单据体名称</param>
        /// <param name="condition">条件</param>
        /// <param name="fieldName">编号属性</param>
        /// <returns></returns>
        private static long GetMaxNumToday(ApiTask task, string bizName, SData condition, string fieldName = "dcode")
        {
            var biz = task.Biz(bizName);
            if (condition == null)
            {
                condition = new SData();
            }

            condition.Append("batch", DateTime.Today.ToString("yyMMdd") + "*");
            // AND m.s0 like '200423%'
            condition.Append("q.orderby", "descid");
            var sql = biz.QuerySql(condition, new List<string>() { fieldName });
            sql = sql.Replace("select", "select top 1 ");
            var maxDecode = biz.ApiTask.DB.ExecuteScalar(sql);
            long maxNum = 0;
            if (maxDecode != null)
            {
                try
                {
                    maxNum = long.Parse(maxDecode.ToString()) + 1;
                }
                catch (Exception)
                {
                    maxNum = long.Parse(System.DateTime.Today.ToString("yyMMdd") + "1".PadLeft(6, '0'));
                }
            }
            else
            {
                maxNum = long.Parse(System.DateTime.Today.ToString("yyMMdd") + "1".PadLeft(6, '0'));
            }

            return maxNum;
        }

        /// <summary>
        /// 下设科目对账属性
        /// </summary>
        /// <param name="biz">实体类（this）</param>
        public static void AddRecoField(BizQuery biz)
        {
            //var tcode = biz.ApiTask.Tcode().FirstOrDefault(m => m.Key == ConstResource.RecoTcode);
            //if (!string.IsNullOrEmpty(tcode.Key))
            //{
            //    biz.AddField(tcode.Key, "下设" + tcode.Value.ToString().Sp_Last(), EFieldType.关联, tcode.Value.ToString().Sp_First(), ConstResource.RecoTcode);
            //}
            //else
            if (!biz.ListField.ContainsKey(AcConst.RecoTcode))
            {
                biz.AddField(AcConst.RecoTcode, "科目对账", EFieldType.关联, "AcSubRec", AcConst.RecoTcode);
            }
        }

        /// <summary>
        /// 更新勾对状态
        /// <param name="biz">表实体</param>
        /// <param name="batchNo">勾对批次</param>
        /// <param name="subRecoId">科目对账id</param>
        /// <param name="isDelete">是否撤销或者删除记录</param>
        /// </summary>
        public static void UpdateRecoState(BizQuery biz, string batchNo, int subRecoId, bool isDelete = false)
        {

            /* 同一个科目对账
                -- 1) 查询批次下的勾对记录（凭证id）
                -- 2）根据批次下的凭证id，查询出关联的所有勾对记录，聚会勾对金额
                -- 3）关联凭证，查询凭证金额（借方金额 + 贷方金额）
                -- 4）比较 勾对金额和凭证金额，相等（全部勾对），不相等（部分勾对）
                --5）修改该批次下凭证的所有勾对记录的勾对状态
            */
            var account = ConstResourceDB.GetAccount(biz.ApiTask, AcConst.RecoAccount);
            var deleteSql = isDelete ? $" and recod.s0 <> '{batchNo}' " : string.Empty;
            var sqlB = $@"update m 
set a9=vs.id  --勾对状态
from 
voucherd as m  --勾对记录
 join (
	select vd.id,(case when sum(recod.d0)!=min(vd.amountd+vd.amountc) then 'z2' else 'z4' end) as recostate 
	from voucherd as vd  -- 凭证体 
	join voucherd as recod on recod.parent = vd.id and recod.account={account} and recod.t29={subRecoId} {deleteSql} -- 勾对体
	join voucherd as pc on recod.parent=pc.parent and pc.account={account} and pc.s0='{batchNo}' and pc.t29={subRecoId} -- 勾对体
	join others as vs on vs.dclass='AcvState' and vs.id=recod.vstate and vs.dcode='z5' 
	group by vd.id) as vd2 on m.parent=vd2.id --分录
 join others as vs on vs.dclass='AcRecoState' and vs.dcode=vd2.recostate -- 勾对状态
 join others as vs2 on vs2.dclass='AcvState' and vs2.id=m.vstate  -- 单据状态
where 
m.t29={subRecoId} And 
vs2.dcode='z5' And 
m.account={account}";

            biz.ApiTask.DB.ExecuteNonQuery(sqlB);
        }

        /// <summary>
        /// 更新勾对状态(撤销勾对前)
        /// <param name="biz">表实体</param>
        /// <param name="batchNo">勾对批次</param>
        /// <param name="subRecoId">科目对账id</param>
        /// <param name="isDelete">是否撤销或者删除记录</param>
        /// </summary>
        public static void UpdateRecoStateByDelete(BizQuery biz, string batchNo, int subRecoId)
        {
            /* 同一个科目对账
                -- 查询批次下的凭证，凭证下的勾对记录，修改勾对状态为部分勾对
            */
            var account = ConstResourceDB.GetAccount(biz.ApiTask, AcConst.RecoAccount);
            var vstateId = ConstResourceDB.GetId(biz.ApiTask, nameof(AcVstate), AcConst.Trial);
            var rstateId = ConstResourceDB.GetId(biz.ApiTask, nameof(AcRecoState), AcConst.PartTick);
            var sqlB = $@"update m 
set a9={rstateId}  --勾对状态
from 
voucherd as m --勾对记录
 join voucherd as pc on m.parent=pc.parent and pc.account={account} and pc.s0='{batchNo}' and pc.t29={subRecoId} -- 勾对体
where 
m.t29={subRecoId} And 
m.vstate={vstateId} And 
m.account={account}";

            biz.ApiTask.DB.ExecuteNonQuery(sqlB);
        }

        /// <summary>
        /// 对多个id进行分页拆分
        /// </summary>
        /// <param name="parentIdList">id集合</param>
        /// <param name="pagesize">每页300</param>
        /// <returns>ids</returns>
        public static SData GetIdsByPage(List<string> parentIdList, int pagesize = 300)
        {
            var totalPage = Math.Ceiling(parentIdList.Count / pagesize * 1.0);
            var i = 0;
            var idData = new SData();
            while (i <= totalPage)
            {
                idData.Add(i.ToString(), parentIdList.Skip(i * pagesize).Take(pagesize).ToList());
                i++;
            }
            return idData;
        }

        /// <summary>
        /// 判断数据体中 科目、类定义启用的tcode属性值是否合法
        /// </summary>
        /// <param name="task">实体类（this.ApiTask）</param>
        /// <param name="account">科目实体</param>
        /// <param name="entity">数据实体</param>
        /// <param name="isRequire">Tcode必填true，Tcode不必填false</param>
        /// <param name="isThrowException">遇到一个异常是否终止</param>
        /// <returns>错误信息集合</returns>
        public static IEnumerable<string> CheckTcode(ApiTask task, SData account, SData entity, bool isRequire = true, bool isThrowException = true)
        {
            // * 判断单据体中 科目、类定义启用的tcode属性值是否合法
            string matchTCode = Regx.TcodeModel;
            var props = account.Where(x => Regex.IsMatch(x.Key, matchTCode));

            // 系统启用的Tcode
            var sysTcodeList = task.Tcode();
            var errorMsg = new List<string>();
            foreach (var prop in props)
            {
                // Tcode启用
                bool isEnable = prop.Value.ToString().ToLower() == "true";

                // Tcode有值
                var tcodeValue = entity.Item<string>(prop.Key);

                // 系统启用了Tcode  todo 测试null
                var sysTcode = sysTcodeList.FirstOrDefault(x => x.Key == prop.Key);

                if (prop.Value != null && isEnable && sysTcode.Value != null)
                {
                    string bizName = sysTcode.Value.ToString().Sp_Last();

                    // Tcode数据无值
                    if (string.IsNullOrEmpty(tcodeValue) && isRequire)
                    {
                        string mesg = bizName + task.LEnd(PubLang.NotEmpty);
                        if (!isThrowException)
                        {
                            errorMsg.Add(mesg);
                        }
                        else
                        {
                            throw new Exception(mesg);
                        }
                    }

                    if (!string.IsNullOrEmpty(tcodeValue))
                    {
                        // 判断数据是否存在
                        string bizCode = sysTcode.Value.ToString().Sp_First();
                        var biz = task.Biz<BizTableCode>(bizCode);
                        var idAuto = biz.GetIDAuto(tcodeValue.Sp_First());
                        if (idAuto <= 0)
                        {
                            string mesg = bizName + task.LEnd(PubLang.NotExists);
                            if (!isThrowException)
                            {
                                errorMsg.Add(mesg);
                            }
                            else
                            {
                                throw new Exception(mesg);
                            }
                        }
                    }
                }
            }

            return errorMsg;
        }

        #endregion

        #region 往来对账和银行对账公共

        /// <summary>
        /// 勾对头编号
        /// </summary>
        private static Dictionary<string, int> AcRecoNumDic = new Dictionary<string, int>();

        private static int AcRecoNum = 0;

        /// <summary>
        /// 获取勾对头Id(每年产生一个新的勾对头)
        /// 1、获取往来勾对头Id(每年产生一个新的往来勾对头)（默认）
        /// 2、获取银行对账单头Id(每年产生一个新的银行对账单头)
        /// </summary>
        /// <param name="task">ApiTask</param>
        /// <param name="suffix">key后缀</param>
        /// <param name="bizName">实体名</param>
        /// <returns>勾对头Id</returns>
        public static int GetRecoId(ApiTask task, string suffix = "AcReco", string bizName = nameof(AcReco))
        {
            string key = $"{task.Domain}_{suffix}";
            lock (_Sync) // 防止并发，进行锁操作
            {
                if (!AcRecoNumDic.ContainsKey(key))
                {
                    AcRecoNumDic.Add(key, 1);
                }

                if (AcRecoNumDic[key] != DateTime.Today.Year)
                {

                    // 从数据库读取
                    var biz = task.Biz<BizTable>(bizName);
                    var recoItem = biz.GetItemByParms(
                        new SData("dcode", DateTime.Today.ToString("yyyy") + suffix).toParmStr(),
                        "id");
                    if (recoItem != null)
                    {
                        AcRecoNum = recoItem.Item<int>("id");
                    }
                    else
                    {
                        SData data = new SData("vclass", AcConst.ActData, "vstate", AcConst.Trial, "dcode", DateTime.Today.ToString("yyyy") + suffix, "subunit", AcConst.UnitHead, "mdate", DateTime.Now.ToString("yyyy-MM-dd"), "bdate", DateTime.Now.ToString("yyyy-MM-dd"), "period", PeriodHelper.GetCurrent(task));

                        // vclass:数据类型(实际数据),vtype:凭证类型,dcode:凭证编号,mdate:制单日期,period:核算期,vstate:状态(已审)
                        data.Append("vtype", task.Biz<BizTable>("AcVoucherType").GetItemByParms(new SData("id", string.Empty).toParmStr(), "dcode")["dcode"]);
                        AcRecoNum = biz.Insert(data);
                    }

                    AcRecoNumDic[key] = DateTime.Today.Year;
                }
            }

            return AcRecoNum;
        }

        /// <summary>
        /// 勾对或收付款时-修改凭证头和凭证体子状态为锁定
        /// </summary>
        /// <param name="biz">表实体</param>
        /// <param name="idList">凭证id集合</param>
        public static void UpdateVSubstateLock(BizQuery biz, List<int> idList)
        {
            if (idList?.Count > 0)
            {
                var trialId = ConstResourceDB.GetId(biz.ApiTask, nameof(AcVstate), AcConst.Trial); // 记账状态下子状态可以为正常或锁定
                var idsql = string.Empty;
                idList.ForEach(x =>
                {
                    idsql += $"id={x} or ";
                });
                var rstateId = ConstResourceDB.GetId(biz.ApiTask, nameof(AcVSubstate), AcConst.LockS);
                var sql = $@"update voucherd set vsubstate=(case when vstate={trialId} then {rstateId} else vsubstate end) where ({idsql} 1=0);
update m set vsubstate=(case when vstate={trialId} then {rstateId} else vsubstate end) from voucher m where exists(select vh from voucherd where ({idsql} 1=0) and vh=m.id);";
                biz.ApiTask.DB.ExecuteNonQuery(sql);
            }
        }

        /// <summary>
        /// 勾对或收付款时-修改凭证头和凭证体子状态为正常
        /// </summary>
        /// <param name="biz">表实体</param>
        /// <param name="idList">凭证id集合</param>
        /// <param name="rrpIdList">收款id或者付款id或勾对记录id集合</param>
        public static void UpdateVSubstateNoLock(BizQuery biz, List<int> idList, List<int> rrpIdList)
        {
            if (idList?.Count > 0 && rrpIdList?.Count > 0)
            {
                // 查询该凭证是否有其他勾对记录或者收付款单
                var idsql = string.Empty;
                var parentsql = string.Empty;
                var noIdsql = string.Empty;
                idList.ForEach(x =>
                {
                    idsql += $"d.id={x} or ";
                    parentsql += $"parent={x} or ";
                    noIdsql += $"id!={x} and ";
                });

                var rrpIdSql = string.Empty;
                rrpIdList.ForEach(x =>
                {
                    rrpIdSql += $"id!={x} and ";
                });

                var trialId = ConstResourceDB.GetId(biz.ApiTask, nameof(AcVstate), AcConst.Trial); // 记账状态下子状态可以为正常或锁定
                var rstateId = ConstResourceDB.GetId(biz.ApiTask, nameof(AcVSubstate), AcConst.NormalS);
                var rstateLockId = ConstResourceDB.GetId(biz.ApiTask, nameof(AcVSubstate), AcConst.LockS);
                var sql = $@"update d 
set vsubstate=(case when d.vstate={trialId} then {rstateId} else d.vsubstate end) 
from 
voucherd as d 
where 
({idsql}1=0) And 
not exists(select parent from voucherd where ({parentsql}1=0) and ({rrpIdSql}1=1) and d.id=parent);
update m 
set vsubstate=(case when m.vstate={trialId} then {rstateId} else m.vsubstate end) 
from 
voucher as m 
where 
exists(select d.vh 
    from
    voucherd d
    where
    ({idsql}1=0) and
    not exists(select parent from voucherd where ({parentsql}1=0) and ({rrpIdSql}1=1) and d.id=parent) and
    vh=m.id) And 
not exists(select vh from voucherd where vh=m.id and vsubstate={rstateLockId});";
                biz.ApiTask.DB.ExecuteNonQuery(sql);
            }
        }

        /// <summary>
        /// 校验Decimal类型的金额或数字
        /// 1、判断金额或数字不能为空
        /// 2、判断金额或数字不能为0（isCheckZero为false可不校验）
        /// 3、判断金额或数字为数值
        /// </summary>
        /// <param name="apiTask">访问api</param>
        /// <param name="decVal">decimal类型的金额或数字</param>
        /// <param name="filedName">字段名称</param>
        /// <param name="isCheckZero">是否校验金额为0</param>
        public static void CheckDecimal(ApiTask apiTask, string decVal, string filedName, bool isCheckZero = true)
        {
            // * 判断金额不能为空
            if (string.IsNullOrEmpty(decVal))
            {
                throw new Exception($"{filedName}{apiTask.LEnd(PubLang.NotEmpty)}");
            }

            try
            {
                decimal amountDec = decVal.ToDec();

                // * 判断金额不能为0
                if (amountDec == 0 && isCheckZero)
                {
                    throw new Exception($"{filedName}{apiTask.LEnd("不能为0")}");
                }
            }
            catch (InvalidCastException)
            {
                // * 判断金额为非数字、空值
                throw new Exception($"{filedName}{apiTask.LEnd("为数值")}");
            }
        }

        #endregion

        #region 银行对账

        /// <summary>
        /// 获取银行科目启用银行账户Tcode
        /// </summary>
        /// <param name="acountIds">一个或多个科目id</param>
        /// <param name="apiTask">访问api</param>
        /// <returns>银行科目启用银行账户Tcode</returns>
        public static SData GetBCTcode(string acountIds, ApiTask apiTask)
        {
            // 将Tcode定义中启用的银行账户的Tcode查询出来
            var acDefTcodeItem = apiTask.Biz<AcDefTcode>(nameof(AcDefTcode)).GetItemByParms(new SData("isenable", true, "biz", $"{nameof(AcBank)}.*").toParmStr(), "dcode");
            if (acDefTcodeItem != null && !string.IsNullOrEmpty(acDefTcodeItem.Item<string>("dcode")))
            {
                var tFiledName = acDefTcodeItem.Item<string>("dcode").Replace('z', 't');

                // 根据科目和银行对账Tcode查询科目
                var accountList = apiTask.BizTableCode("AcAccount").GetListData(0, 1, new SData("id", acountIds, tFiledName, true).toParmStr(), $"id,{tFiledName}");
                var tcodeData = new SData();
                if (accountList != null && accountList.Count > 0)
                {
                    foreach (var account in accountList)
                    {
                        tcodeData.Append(account.Item<string>("id"), tFiledName);
                    }

                    return tcodeData;
                }
            }

            return new SData();
        }

        /// <summary>
        /// 修改企业凭证和对账单的勾对金额、勾对类型、勾对人、勾对日期
        /// 修改凭证和对账单子状态为锁定
        /// </summary>
        /// <param name="biz">表实体</param>
        /// <param name="recoAmount">勾对金额</param>
        /// <param name="recoType">勾对类型</param>
        /// <param name="loginUser">勾对人</param>
        /// <param name="corpVDId">企业凭证id</param>
        /// <param name="bankVDId">对账单Id</param>
        /// <returns></returns>
        public static int UpdateVoucherD(BizQuery biz, decimal recoAmount, string recoType, string loginUser, int corpVDId, int bankVDId)
        {
            var trialId = ConstResourceDB.GetId(biz.ApiTask, nameof(AcVstate), AcConst.Trial); // 记账状态下子状态可以为正常或锁定
            var rstateId = ConstResourceDB.GetId(biz.ApiTask, nameof(AcVSubstate), AcConst.LockS);
            var sql = $@"update m 
set a9=(select id from others where dclass='acrecostate' and dcode=(case when (isnull(m.d0,0)+{recoAmount})!=(m.amountd+m.amountc) then '{AcConst.PartTick}' else '{AcConst.AllTick}' end)),
d0=(isnull(m.d0,0)+{recoAmount}),
a8=(select id from others where dclass='acrecotype' and dcode='{recoType}'),
a7=(select id from users where dclass='user' and dcode='{loginUser}'),
s0=(case when m.id={corpVDId} then '{AcConst.BigDate}' else '{DateTime.Now.ToString("yyyy-MM-dd")}' end),
a4={corpVDId},
vsubstate=(case when m.vstate={trialId} then {rstateId} else m.vsubstate end)  
from 
voucherd as m 
where m.id={corpVDId} or m.id={bankVDId};
update m 
set vsubstate=(case when m.vstate={trialId} then {rstateId} else m.vsubstate end) 
from 
voucher as m 
where 
exists(select vh from voucherd where id={corpVDId} and vh=m.id);";

            return biz.ApiTask.DB.ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 截取指定长度的中英文混合字符串
        /// </summary>
        /// <param name="s">当前字符</param>
        /// <param name="l">指定长度</param>
        /// <param name="endStr">结尾字符</param>
        /// <returns>截取后的字符</returns>
        public static string GetStr(string s, int l, string endStr = "")
        {
            // 当截取的字符为空或者实际长度小于l，直接返回
            if (string.IsNullOrEmpty(s) || s.GetLength() <= l)
            {
                return s;
            }

            // 循环截取中英文长度
            string temp = s.Substring(0, (s.Length < l + 1) ? s.Length : l + 1);
            byte[] encodedBytes = System.Text.Encoding.ASCII.GetBytes(temp);

            string outputStr = string.Empty;
            int count = 0;

            for (int i = 0; i < temp.Length; i++)
            {
                if ((int)encodedBytes[i] == 63)
                {
                    count += 2;
                }
                else
                {
                    count += 1;
                }

                if (count <= l - endStr.Length)
                {
                    outputStr += temp.Substring(i, 1);
                }
                else if (count > l)
                {
                    break;
                }
            }

            if (count <= l)
            {
                outputStr = temp;
                endStr = string.Empty;
            }

            outputStr += endStr;

            return outputStr;
        }
        #endregion

    }
}