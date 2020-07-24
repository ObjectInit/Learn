#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcVoucherDSub.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcVoucherDSub
* 创 建 者：曾倩倩
* 创建时间：2019/10/24 9:12:57
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 勾对凭证分录
    /// 1 描述：用于查询“选择勾对数据”、“校验收款数据合法性”、“校验付款数据合法性”
    /// 2 约定：
    ///     1.查询时使用checkCan标识校验收款/付款数据合法性，使用subrec存储科目对账，使用dList存储凭证分录列表
    ///     2.查询时使用subrec标识“选择勾对数据”，根据subrec、recostate、notickamount、id、billtype查询凭证分录列表
    ///     3.使用getno查询应收应付的对方金额，使用details查询未勾对明细的应收应付
    ///     4.查询时约定key：calctickamount，后端将返回分录的提审金额和已勾对金额
    /// 3 业务逻辑：
    ///     查询前：校验付款数据合法性、校验收款数据合法性、查询“选择勾对数据”
    /// </summary>
    [ClassData("cname", "凭证分录")]
    public class AcVoucherDSub : AcVoucherD
    {
        /// <summary>
        /// 参数、子查询定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // * 查询参数定义
            this.AddParm("subrecid", "科目对账");
            this.AddParm("recostate", "勾对状态");
            this.AddParm("details", "未勾对明细");
            this.AddParm("notickamount", "未勾对金额");
            this.AddParm("subused", "科目是否已对账");

            // 删除不需要的字段
            this.ListField.Remove("isori"); // 原始标记
        }

        #region 框架方法重写

        /// <summary>
        /// 默认参数：状态已审，数据类型为实际数据
        /// </summary>
        public override SData BaseParms => new SData("vstate", AcConst.Trial, "vclass", AcConst.ActData);

        /// <summary>
        /// 自定义查询参数的sql处理
        /// 1、处理科目对账查询参数
        /// 2、处理勾对状态查询参数
        /// 3、处理应收应付查询参数
        /// 4、处理未勾对金额不为0查询参数
        /// 5、处理科目是否已对账查询参数
        /// </summary>
        /// <param name="parmKey">参数key</param>
        /// <param name="parmStr">参数value</param>
        /// <returns></returns>
        protected override string CustomParmSql(string parmKey, string parmStr)
        {
            string sql = base.CustomParmSql(parmKey, parmStr);

            // * 处理科目对账查询参数
            if (parmKey == "subrecid")
            {
                var subRecId = AcVoucherHelper.GetSubRecId(parmStr, this.ApiTask); // 科目对账
                SubRec = $"[{subRecId}]";
                sql += $@" 
exists (select 1 
    from 
    others 
    where 
    a9 = {subRecId} and 
    a8 = {{0}}.account and 
    dclass = 'acreconref')";
            }

            // * 处理勾对状态查询参数
            else if (parmKey == "recostate")
            {
                // 数据准备 **********
                // 变量声明
                var subrecId = AcVoucherHelper.GetSubRecId(SubRec, this.ApiTask); // 科目对账

                // 勾对科目
                int recoAccountId = ConstResourceDB.GetAccount(this.ApiTask, AcConst.RecoAccount);

                // 已审状态
                int trialSql = ConstResourceDB.GetId(this.ApiTask, nameof(AcVstate), AcConst.Trial);

                // 查询“凭证体下的勾对记录列表”的sql语句
                string sqlCount = $@"(
select 1 
from 
voucherd as bud 
where 
bud.parent = {{0}}.id and 
bud.{AcConst.RecoTcode} = {subrecId} and 
bud.account = {recoAccountId} and 
bud.vstate = {trialSql} ";

                if (!string.IsNullOrEmpty(parmStr))
                {
                    parmStr = parmStr.Sp_First();
                }

                // 勾对状态
                int recoStateSql = GetId(this.ApiTask, nameof(AcRecoState), parmStr.Sp_First());
                string code = GetCode(this.ApiTask, nameof(AcRecoState), parmStr.Sp_First());

                // 从未勾对
                if (code == AcConst.NeverTick)
                {
                    // 科目对账中，凭证体下的已审状态的勾对记录列表为空
                    sqlCount += ")";
                    sql += $@" 
not exists {sqlCount} ";
                }
                else if (code == AcConst.PartTick) // 部分勾对
                {
                    // 科目对账中，凭证体下的已审状态的勾对记录列表有记录，且勾对记录中的勾对状态为部分勾对
                    sqlCount += $" and bud.a9 = {recoStateSql} )";
                    sql += $" exists {sqlCount} ";
                }
                else if (code == AcConst.NoAllTick) // 未勾对
                {
                    // 科目对账中，（凭证体下的已审状态的勾对记录列表为空） 或者 （凭证体下的已审状态的勾对记录列表有值，且勾对记录中的勾对状态为部分勾对）
                    recoStateSql = ConstResourceDB.GetId(this.ApiTask, nameof(AcRecoState), AcConst.PartTick);
                    string sqlcount1 = sqlCount + $" and bud.a9 = {recoStateSql} )";
                    sql += $@" 
(not exists {sqlCount}) or 
(exists {sqlcount1}))";
                }
                else if (code == AcConst.AllTick) // 全部勾对
                {
                    // 科目对账中，凭证体下的已审状态的勾对记录列表有值，且勾对记录中的勾对状态为全部勾对
                    sqlCount += $@" and 
bud.a9 = {recoStateSql} )";
                    sql += $@"  
exists {sqlCount} ";
                }
                else if (code == AcConst.AlreadyTick) // 已勾对（部分勾对和全部勾对）
                {
                    // 科目对账中，凭证体下的已审状态的勾对记录列表有值
                    sqlCount += $")";
                    sql += $@" 
exists {sqlCount} ";
                }
            }

            // * 处理应收应付查询参数
            else if (parmKey == "details")
            {
                // 科目对账 a9-科目对账，a8-科目，a7-单据类型，a6-是否反向
                var subrecId = AcVoucherHelper.GetSubRecId(parmStr, this.ApiTask); // 科目对账
                SubRec = $"[{subrecId}]";
                sql += $@"  
(({{0}}.cd = {AcConst.Debit} and 
exists (select 1 
    from 
    others as os 
    where 
    os.dclass = 'acreconref' and 
    os.a8={{0}}.account and 
    os.a9 = {subrecId} and 
    exists (select 1 
        from 
        others as os1 
        where 
        os1.dclass = 'acbilltype' and 
        os1.id=os.a7 and 
        os1.dcode = '{AcConst.ReceiptVoucher}'))) Or 
({{0}}.cd = {AcConst.Credit} and 
exists (select 1 
    from 
    others as os2 
    where 
    os2.dclass = 'acreconref' and 
    os2.a8={{0}}.account and 
    os2.a9 = {subrecId} and 
    exists (select 1 
        from 
        others as os3 
        where 
        os3.dclass = 'acbilltype' and 
        os3.id = os2.a7 and 
        os3.dcode = '{AcConst.PayVoucher}'))))";
            }

            // * 处理未勾对金额不为0
            else if (parmKey == "notickamount")
            {
                // 查询“勾对记录中的勾对金额（包含提审金额）和”的sql语句
                var subrecId = AcVoucherHelper.GetSubRecId(SubRec, this.ApiTask); // 科目对账
                int recoAccountId = ConstResourceDB.GetAccount(this.ApiTask, AcConst.RecoAccount); // 勾对科目

                string sqlRCAmount = $@"(
select isnull(sum(rd.d0),0) 
from 
voucherd as rd 
where 
rd.account = {recoAccountId} and 
rd.parent = {{0}}.id and 
rd.{AcConst.RecoTcode} = {subrecId})";
                sql += $@" 
({{0}}.id = {ReplaceInToOr(parmStr)}) and 
{{0}}.amountc + {{0}}.amountd != {sqlRCAmount}";
            }

            // * 科目是否已对账（用于修改/删除科目对账时，判断能否修改/删除）
            else if (parmKey == "subused")
            {
                if (!string.IsNullOrEmpty(parmStr))
                {
                    string subrecId = parmStr.Sp_First();
                    string accountId = parmStr.Sp_Last();
                    int recoAccountId = ConstResourceDB.GetAccount(this.ApiTask, AcConst.RecoAccount); // 勾对科目
                    if (!string.IsNullOrEmpty(subrecId) && !string.IsNullOrEmpty(accountId))
                    {
                        sql += $@" 
{{0}}.account = {accountId} and 
exists (select 
    parent 
    from 
    voucherd as reco 
    where 
    reco.dclass='0' and 
    reco.account = {recoAccountId} and 
    reco.parent = {{0}}.id and 
    reco.{AcConst.RecoTcode} = {subrecId})";
                    }
                }
            }

            return sql;
        }

        /// <summary>
        /// 查询前
        /// 1、校验是否允许付款、收款
        /// 2、查询“选择勾对数据”页面的凭证分录列表：
        ///     1、根据单据类型获取cd条件
        ///     2、根据科目对账、单据类型、科目获取科目集合
        ///     3、根据未勾对金额不为0过滤凭证分录Id
        /// </summary>
        /// <param name="QParm">查询条件</param>
        protected override void OnGetListBefore(SData QParm)
        {
            // 业务逻辑 **********
            // * 校验是否允许付款、收款
            if (!string.IsNullOrEmpty(QParm.Item<string>("checkCan")))
            {
                VoucherHelper.CheckCanPayReceipt(QParm, this, QParm["checkCan"].ToString().ToLower() == "true", true);

                QParm.Clear();
                QParm.Add("id", "0");
            }
            else if (!string.IsNullOrEmpty(QParm.Item<string>("subrec"))) // * 查询“选择勾对数据”页面的凭证分录列表
            {
                // 得到科目对账和科目关系的对象
                SubRec = QParm["subrec"].ToString();
                QParm.Remove("subrec");

                // 根据单据类型获取cd条件
                string billType = string.Empty;
                if (!string.IsNullOrEmpty(QParm.Item<string>("billtype")))
                {
                    billType = QParm["billtype"].ToString();

                    // 未勾对明细页面
                    if (!QParm.ContainsKey("getno"))
                    {
                        // 如果是收款单，金额在借方且不等于0，忽略贷方金额有值的
                        if (billType.Sp_First() == AcConst.ReceiptVoucher)
                        {
                            QParm["cd"] = AcConst.Debit;
                        }
                        else if (billType.Sp_First() == AcConst.PayVoucher) // 如果是付款单，金额在贷方且不等于0，忽略借方有值的
                        {
                            QParm["cd"] = AcConst.Credit;
                        }
                    }
                }

                QParm["account.id"] = VoucherHelper.GetAccountBySubrec(this.ApiTask, SubRec, billType, QParm.Item<string>("account"));
            }

            // 处理核算单位
            this.ApiTask.SubUnitProcess(QParm);

            // 处理日期条件的区间 b:e
            QParm.ConvertDate("bdate");
            QParm.ConvertDate("period");

            // 命中索引
            if (!QParm.ContainsKey("period"))
            {
                QParm["period"] = "190001:209901";
            }

            base.OnGetListBefore(QParm);
        }

        /// <summary>
        /// 查询后处理数据
        /// 1、批量计算提审金额
        /// 2、批量计算已勾对金额
        /// </summary>
        /// <param name="QParm">查询参数</param>
        /// <param name="Data">数据集合</param>
        protected override void OnGetListAfter(SData QParm, List<SData> Data)
        {
            base.OnGetListAfter(QParm, Data);

            // 后端批量计算已勾对金额
            if (QParm.ContainsKey("calctickamount") && Data.Count > 0)
            {
                var ids = new SData();
                int index = 0;
                string iddd = string.Empty;
                foreach (var id in Data.Select(m => m.Item<string>("id")))
                {
                    index++;
                    double numKey = index / 100.0;
                    ids[Math.Ceiling(numKey).ToString()] += "," + id;
                    iddd += "," + id;
                }

                // 科目为勾对科目
                string accountSql = $@" left join account as m_account on m.account = m_account.id and m_account.dclass = 'a' ";

                // 单据状态
                string stateSql = $@" left join others as m_state on m.vstate = m_state.id and m_state.dclass= 'acvstate' ";

                var subRecid = SubRec.Replace("[", string.Empty).Replace("]", string.Empty);

                foreach (var id in ids.Keys)
                {
                    var parent = ids[id].ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    StringBuilder sb = new StringBuilder(@" and
(parent=-1");
                    foreach (var item in parent)
                    {
                        sb.Append(" or parent=" + item);
                    }

                    sb.Append(")");
                    var vid = string.Join(",", Data.Select(m => m["id"]));

                    // 提审金额
                    string sql = $@"select 
isnull(sum(m.d0),0) as amountp,
m.parent 
from 
voucherd as m 
{accountSql}
{stateSql} 
where 
m.dclass = '0' and 
m.{AcConst.RecoTcode} = {subRecid}{sb.ToString()} and 
m_account.dcode = '{AcConst.RecoAccount}' and 
m_state.dcode < '{AcConst.Trial}' 
group by m.parent";
                    var amountpData = this.ApiTask.DB.ExecuteList(sql);

                    // 赋值提审金额
                    amountpData.ForEach(m =>
                    {
                        var tempM = Data.FirstOrDefault(v => v.Item<string>("id").Equals(m.Item<string>("parent")));
                        tempM?.Append("amountp", m["amountp"]);
                    });

                    // 已勾对金额
                    sql = $@"select 
isnull(sum(m.d0),0) as recoamount,
m.parent 
from 
voucherd as m 
{accountSql} 
{stateSql} 
where 
m.dclass = '0' and 
m.{AcConst.RecoTcode} = {subRecid} {sb.ToString()} and 
m_account.dcode = '{AcConst.RecoAccount}' and 
m_state.dcode = '{AcConst.Trial}' 
group by m.parent";
                    var sumRecoamountData = this.ApiTask.DB.ExecuteList(sql); // 已勾对金额

                    // 赋值已勾对金额
                    sumRecoamountData.ForEach(m =>
                    {
                        var tempM = Data.FirstOrDefault(v => v.Item<string>("id").Equals(m.Item<string>("parent")));
                        tempM?.Append("recoamount", m["recoamount"]);
                    });
                }
            }
        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 科目对账id
        /// </summary>
        private string SubRec = string.Empty;

        private SData codeData = new SData();

        /// <summary>
        /// 状态、类别等其他代码对象缓存（存储在others表中）
        /// </summary>
        /// <param name="apiTask">apitask</param>
        /// <param name="dclass">类名称</param>
        /// <param name="dcodeId">代码或id</param>
        /// <returns></returns>
        private string GetCode(ApiTask apiTask, string dclass, string dcodeId)
        {
            // 变量声明
            var key = $"{apiTask.Domain}_{dclass}"; // 将账套以及类代码作为key存储起来
            if (!codeData.ContainsKey(key))
            {
                codeData.Add(key, new SData());
                var listData = apiTask.DB.ExecuteList(@"select id,dcode from others where dclass='" + dclass.Trim() + "'");
                listData.ForEach(m =>
                {
                    ((SData)codeData[key]).Append(m.Item<string>("dcode"), m.Item<string>("dcode"));
                    ((SData)codeData[key]).Append($"[{m.Item<int>("id")}]", m.Item<string>("dcode"));
                });
            }

            return ((SData)codeData[key]).Item<string>(dcodeId);
        }

        private SData idData = new SData();

        /// <summary>
        /// 状态、类别等其他代码对象缓存（存储在others表中）
        /// </summary>
        /// <param name="apiTask">apitask</param>
        /// <param name="dclass">类名称</param>
        /// <param name="dcodeId">代码或id</param>
        /// <returns></returns>
        private int GetId(ApiTask apiTask, string dclass, string dcodeId)
        {
            // 变量声明
            var key = $"{apiTask.Domain}_{dclass}"; // 将账套以及类代码作为key存储起来
            if (!idData.ContainsKey(key))
            {
                idData.Add(key, new SData());
                var listData = apiTask.DB.ExecuteList(@"select id,dcode from others where dclass='" + dclass.Trim() + "'");
                listData.ForEach(m =>
                {
                    ((SData)idData[key]).Append(m.Item<string>("dcode"), m.Item<int>("id"));
                    ((SData)idData[key]).Append($"[{m.Item<int>("id")}]", m.Item<int>("id"));
                });
            }

            return ((SData)idData[key]).Item<int>(dcodeId);
        }

        private string ReplaceInToOr(string ids)
        {
            return ids.Replace(",", " or {0}.id = ");
        }

        #endregion
    }
}
