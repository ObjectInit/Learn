#region  <<版本注释>>
/* ========================================================== 
// <copyright file="AcCorpVD.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcCorpVD 
* 创 建 者：张莉 
* 创建时间：2020/5/6 9:44:24 
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
    /// 企业账和银行账
    /// 1 描述：企业账和银行账
    /// 2 约定：
    ///     1.继承AcVoucherD，dclass是0
    ///     2.定义企业凭证预留字段
    ///     3.提供选择勾对数据、修改勾对金额GetList接口（qParm中没有type）
    ///     4.提供自动勾对（qParm中type=z3）、手动勾对（qParm中type=z1）、勾对记录（qParm中type=brdata和bparent=(1)）和撤销勾对（qParm中type=brdel）GetList接口
    ///     5.勾对状态查询约定（key为recostate，value为dcode或dcode.title）
    ///     6.查询企业凭证和银行对账单集合约定（key为bankreco，value为银行对账id）
    ///     7.银行科目查询约定（key为bkaccountVD，value任意）
    ///     8.银行账户查询约定（key为bankVD，value任意）
    /// 3 业务逻辑：
    ///     查询前：
    ///     1.自动勾对（type=z3）或手动勾对（type=z1）或撤销勾对（type=brdel），添加id=-1参数，不查询数据
    ///     2.勾对记录--按照父id升序和勾对日期降序排序
    ///     3.选择勾对数据和修改勾对金额--按照业务日期和id升序排序
    ///     查询后：
    ///     1.qParm中type=z3 自动勾对接口，返回提示信息:(自动勾对完成,成功勾对XX条数据,勾对金额XX元)
    ///     2.qParm中type=z1 手动勾对接口
    ///     3.qParm中type=brdel 撤销勾对接口
    /// </summary>
    [ClassData("cname", "企业银行账")]
    public class AcCorpVD : AcVoucherD
    {
        /// <summary>
        /// 字段定义、查询参数定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // *字段定义
            // 启用预留字段（企业银行账凭证体冗余勾对字段）
            this.AddField("rstate", ApiTask.L("勾对状态"), EFieldType.关联, "AcRecoState", "a9");
            this.AddField("recoamount", ApiTask.L("勾对金额"), EFieldType.数值, string.Empty, "d0");
            this.AddField("logicsign", ApiTask.L("勾对类型"), EFieldType.关联, "ACRecoType", "a8");
            this.AddField("recoperson", ApiTask.L("勾对人"), EFieldType.关联, "AcUser", "a7");
            this.AddField("recodate", ApiTask.L("勾对日期"), EFieldType.字符串, string.Empty, "s0");
            this.AddField("bparent", ApiTask.L("银行父Id"), EFieldType.整数, string.Empty, "a4");

            // * 查询参数定义
            // 勾对状态查询
            this.AddParm("recostate", "勾对状态");

            // 查询企业对账单和银行对账单
            this.AddParm("bankreco", "银行对账");

            // 银行科目->查询企业对账单和银行对账单
            this.AddParm("bkaccountVD", "银行科目查询");

            // 银行账户->查询企业对账单和银行对账单
            this.AddParm("bankVD", "银行账户查询");
        }

        #region 框架方法重写

        /// <summary>
        /// 自定义查询参数的sql处理
        /// 1、处理勾对状态查询参数
        /// </summary>
        /// <param name="parmKey">参数key</param>
        /// <param name="parmStr">参数value</param>
        /// <returns></returns>
        protected override string CustomParmSql(string parmKey, string parmStr)
        {
            string sql = base.CustomParmSql(parmKey, parmStr);

            // 查询勾对状态的数据
            if (parmKey == "recostate")
            {
                switch (parmStr.Sp_First())
                {
                    case AcConst.NeverTick: // 从未勾对
                        {
                            // 查询企业凭证/银行对账单中勾对状态为null或者0或者从未勾对的数据
                            int neverTick = ConstResourceDB.GetId(this.ApiTask, nameof(AcRecoState), AcConst.NeverTick);
                            sql += $@"({{0}}.a9 is null or {{0}}.a9=0 or {{0}}.a9={neverTick})";
                            break;
                        }
                    case AcConst.PartTick: // 部分勾对
                        {
                            // 查询企业凭证/银行对账单中勾对状态为部分勾对的数据
                            int partTick = ConstResourceDB.GetId(this.ApiTask, nameof(AcRecoState), AcConst.PartTick);
                            sql += $@"({{0}}.a9={partTick})";
                            break;
                        }
                    case AcConst.NoAllTick: // 未勾对(从未勾对或者部分勾对)
                        {
                            // 查询企业凭证/银行对账单中勾对状态为null或者0或者从未勾对或者部分勾对的数据
                            int neverTick = ConstResourceDB.GetId(this.ApiTask, nameof(AcRecoState), AcConst.NeverTick);
                            int partTick = ConstResourceDB.GetId(this.ApiTask, nameof(AcRecoState), AcConst.PartTick);
                            sql += $@"({{0}}.a9 is null or {{0}}.a9=0 or {{0}}.a9={neverTick} or {{0}}.a9={partTick})";
                            break;
                        }
                    case AcConst.AllTick: // 全部勾对
                        {
                            // 查询企业凭证/银行对账单中勾对状态为全部勾对的数据
                            int allTick = ConstResourceDB.GetId(this.ApiTask, nameof(AcRecoState), AcConst.AllTick);
                            sql += $@"({{0}}.a9={allTick})";
                            break;
                        }
                    case AcConst.AlreadyTick: // 已勾对（包含部分勾对和全部勾对）
                        {
                            // 查询企业凭证/银行对账单中勾对状态为部分勾对和全部勾对的数据
                            int partTick = ConstResourceDB.GetId(this.ApiTask, nameof(AcRecoState), AcConst.PartTick);
                            int allTick = ConstResourceDB.GetId(this.ApiTask, nameof(AcRecoState), AcConst.AllTick);
                            sql += $@"({{0}}.a9={partTick} or {{0}}.a9={allTick})";
                            break;
                        }
                    default: break;
                }
            }
            else if (parmKey == "bankreco")
            {
                // * 查询企业凭证和银行对账单集合
                // 根据银行对账查找核算单位、银行科目、银行账户
                int.TryParse(parmStr, out int banka);
                if (banka <= 0)
                {
                    throw new Exception(ApiTask.LEnd("银行对账不存在"));
                }

                var bankAItem = ApiTask.Biz<BizTable>("AcBankA").GetItem(banka, "id,subunit.id,bkaccount.id,bank.id");

                if (bankAItem == null)
                {
                    throw new Exception(ApiTask.LEnd("银行对账不存在"));
                }

                var bkaccountId = bankAItem.Item<string>("bkaccount.id");
                if (string.IsNullOrEmpty(bkaccountId))
                {
                    throw new Exception(ApiTask.LEnd("银行科目不存在"));
                }

                // 查找银行科目启用的Tcode
                var tcodeData = AcVoucherHelper.GetBCTcode(bkaccountId, ApiTask);
                var tcode = tcodeData.Item<string>(bkaccountId);
                var tcodeSql = !string.IsNullOrEmpty(tcode) ? $"{{0}}.{tcode}={bankAItem["bank.id"]}" : "1=1";

                // 拼接查找企业凭证和银行对账单的sql
                sql += $@" 
(({{0}}.subunit={bankAItem["subunit.id"]} and 
{{0}}.account={bkaccountId} and 
{tcodeSql}) or 
({{0}}.subunit={bankAItem["subunit.id"]} and 
{{0}}.a6={bkaccountId} and 
{{0}}.a5={bankAItem["bank.id"]}))";
            }
            else if (parmKey == "bkaccountVD") // 银行科目->查询企业对账单和银行对账单
            {
                var bankAList = ApiTask.Biz<BizTable>("AcBankA").GetListData(0, 1, $"bkaccount={parmStr}", "bkaccount.id,bank.id").Select(x => x.Item<int>("bkaccount.id")).ToList();
                if (bankAList.Count > 0)
                {
                    // 拼接查找企业凭证和银行对账单的科目sql
                    var accountSql = string.Empty;
                    foreach (var bankA in bankAList)
                    {
                        accountSql += $@"{{0}}.account={bankA} or {{0}}.a6={bankA} or ";
                    }

                    sql += $@"({accountSql}1=0)";
                }
                else
                {
                    sql += " 1=0 ";
                }
            }
            else if (parmKey == "bankVD") // 银行账户->查询企业对账单和银行对账单
            {
                var bankAList = ApiTask.Biz<BizTable>("AcBankA").GetListData(0, 1, $"bank={parmStr}", "bkaccount.id,bank.id");
                var bkaccountIdList = bankAList.Select(x => x.Item<int>("bkaccount.id")).ToList();
                if (bankAList.Count > 0)
                {
                    var tcodeData = AcVoucherHelper.GetBCTcode(String.Join(",", bkaccountIdList), ApiTask);

                    // 拼接查找企业凭证和银行对账单的银行账户sql
                    var bankSql = string.Empty;
                    foreach (var banka in bankAList)
                    {
                        var bkaccount = banka.Item<string>("bkaccount.id");
                        var bank = banka.Item<string>("bank.id");
                        var tcode = tcodeData.Item<string>(bkaccount);
                        var tcodeSql = !string.IsNullOrEmpty(tcode) ? $"{{0}}.{tcode}={bank}" : "1=0";
                        bankSql += $@"{{0}}.a5={bank} or {tcodeSql} or ";
                    }

                    sql += $@"({bankSql}1=0)";
                }
                else
                {
                    sql += " 1=0 ";
                }
            }

            return sql;
        }

        /// <summary>
        /// 查询前
        /// 说明（qParm中type=z3 自动勾对接口
        ///       qParm中type=z1 手动勾对（确定勾对）接口
        ///       qParm中type=brdel 撤销勾对接口
        ///       qParm中type=brdata 勾对记录接口
        ///       qParm中没有type时，为选择勾对数据接口或修改勾对金额接口）
        /// 1、自动勾对（type=z3）或手动勾对（type=z1）或撤销勾对（type=brdel），添加id=-1参数，不查询数据
        /// 2、勾对记录--按照父id升序和勾对日期降序排序
        /// 3、选择勾对数据和修改勾对金额--按照业务日期和id升序排序
        /// </summary>
        /// <param name="qParm">查询参数</param>
        protected override void OnGetListBefore(SData qParm)
        {
            // 数据准备 **********
            // 变量声明
            string type = qParm.Item<string>("type");

            // * 自动勾对（type=z3）或手动勾对（type=z1）或撤销勾对（type=brdel），添加id=-1参数，不查询数据
            if (AcConst.RecoAuto.Equals(type) || AcConst.RecoManual.Equals(type) || AcConst.BRecoDel.Equals(type))
            {
                qParm.Add("id", -1);
                return;
            }

            // 处理核算单位
            this.ApiTask.SubUnitProcess(qParm);

            // 处理日期条件的区间 b:e
            qParm.ConvertDate("bdate");
            qParm.ConvertDate("period");

            // 核算期命中索引
            if (!qParm.ContainsKey("period"))
            {
                qParm["period"] = $"{AcConst.PeriodBegin}:{AcConst.PeriodEnd}";
            }

            if (AcConst.BRecoData.Equals(type))
            {
                // * 勾对记录--按照父id升序和勾对日期降序排序
                qParm.Append("q.orderby", "bparent,descrecodate");

                // 勾对记录标识-默认值
                qParm["bparent"] = qParm["bparent", "(1)"];
                qParm["recostate"] = qParm["recostate", AcConst.AlreadyTick];
            }
            else
            {
                // * 选择勾对数据和修改勾对金额--按照业务日期和id升序排序 -变更为根据贷方金额、借方金额升序
                if (string.IsNullOrEmpty(qParm.Item<string>("q.orderby")))
                {
                    qParm.Append("q.orderby", "amountc,amountd,id"); // todo
                }
            }

            base.OnGetListBefore(qParm);
        }

        /// <summary>
        /// 查询后逻辑
        /// 1、qParm中type=z3 自动勾对接口，返回提示信息:(自动勾对完成,成功勾对XX条数据,勾对金额XX元)
        /// 2、qParm中type=z1 手动勾对（确定勾对）接口
        /// 3、qParm中type=brdel 撤销勾对接口
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
            switch (type)
            {
                case AcConst.RecoAuto:
                    {
                        #region 自动勾对
                        // * 必填
                        // 核算单位必填
                        if (string.IsNullOrEmpty(qParm.Item<string>("subunit")))
                        {
                            throw new Exception($"{ApiTask.L("核算单位")}{ApiTask.LEnd(PubLang.NotEmpty)}");
                        }

                        // 银行科目必填
                        if (string.IsNullOrEmpty(qParm.Item<string>("bkaccount")))
                        {
                            throw new Exception($"{ApiTask.L("银行科目")}{ApiTask.LEnd(PubLang.NotEmpty)}");
                        }

                        // * 核算单位逻辑校验
                        var subunitCode = qParm.Item<string>("subunit").Sp_First(",").Sp_First(".");

                        // 当前核算单位
                        string currentSubunit = ApiTask.UserInfo().UserSubUnit();

                        // 登录用户的核算单位与条件传入的核算单位不一致
                        if (!string.IsNullOrEmpty(currentSubunit) && !currentSubunit.Sp_First(".").Equals(subunitCode))
                        {
                            throw new Exception(ApiTask.LEnd("核算单位不一致，不允许勾对"));
                        }

                        // * 校验银行对账初始化
                        var accountCode = qParm.Item<string>("bkaccount").Sp_First(",").Sp_First(".");
                        var bank = qParm.Item<string>("bank");

                        // 根据核算单位、银行科目、银行账户，查询银行对账集合
                        var bankAList = ApiTask.Biz<BizTable>("AcBankA").GetListData(0, 0, new SData("subunit", subunitCode, "bkaccount", accountCode, "bank", bank).toParmStr(), "id,subunit.id,bkaccount.id,bank.id");
                        if (bankAList == null || bankAList.Count <= 0)
                        {
                            throw new Exception(ApiTask.LEnd("未设置银行对账，不允许勾对"));
                        }

                        string message = AutoReco(qParm, bankAList);
                        Data.Add(new SData("message", message));

                        #endregion
                        break;
                    }
                case AcConst.RecoManual:
                    {
                        #region 手动勾对

                        ManualReco(qParm);

                        #endregion
                        break;
                    }
                case AcConst.BRecoDel:
                    {
                        #region 撤销勾对

                        DeleteReco(qParm);

                        #endregion
                        break;
                    }
                default: break;
            }

            base.OnGetListAfter(qParm, Data);
        }

        /// <summary>
        /// 修改逻辑前
        /// </summary>
        /// <param name="oldData">修改前勾对记录数据</param>
        /// <param name="updateData">修改后勾对记录数据</param>
        protected override void OnUpdateBefore(SData oldData, SData updateData)
        {
            // 将凭证头的id赋给凭证体的vh
            updateData["vh"] = $"[{oldData["vh.id"]}]";
        }

        #endregion

        #region 自定义方法

        #region 自动勾对

        /// <summary>
        /// 自动勾对
        /// 1、变量声明
        /// 2、获取过滤条件
        /// 3、分银行对账进行自动勾对
        /// 4、按业务参考优先勾对
        /// 5、按业务日期小的优先勾对
        /// 6、企业凭证-按业务日期分组查询企业凭证
        /// 7、循环业务日期
        /// 12、返回勾对完成消息
        /// </summary>
        /// <param name="qParm">查询参数</param>
        /// <param name="bankAList">银行对账集合</param>
        /// <returns>勾对完成消息</returns>
        private string AutoReco(SData qParm, List<SData> bankAList)
        {
            // 数据准备 **********
            // *变量声明
            int recoCount = 0; // 记录勾对体成功添加的条数
            decimal recoAmountSum = 0; // 总的勾对金额

            // * 过滤条件
            var filterParm = new SData();  // 企业凭证和银行对账单公共过滤条件
            var corpVDfilterParm = new SData(); // 企业凭证过滤条件
            var bankVDfilterParm = new SData(); // 银行对账单过滤条件

            // 处理日期条件的区间 b:e
            qParm["bdateb"] = qParm["bdateb", "1900-01-01"];
            qParm.ConvertDate("bdate");
            qParm.ConvertDate("period");

            // 核算期命中索引
            if (!qParm.ContainsKey("period"))
            {
                qParm["period"] = "190001:209901";
            }

            filterParm.Append("recostate", AcConst.NeverTick);
            filterParm.Append("bdate", qParm["bdate"]);
            filterParm.Append("period", qParm["period"]);

            // 隐含条件,实际数据,记账状态,
            var vclassId = ConstResourceDB.GetId(this.ApiTask, nameof(AcVclass), AcConst.ActData);
            filterParm.Append("vclass", $"[{vclassId}]");

            var vstateId = ConstResourceDB.GetId(this.ApiTask, nameof(AcVstate), AcConst.Trial);
            filterParm.Append("vstate", $"[{vstateId}]");

            filterParm.Append("q.orderby", "bdate,id");

            // 获取银行科目启用的银行账户Tcode
            var accIds = string.Join(",", bankAList.Select(x => x.Item<string>("bkaccount.id")).ToList());
            var tcodeData = AcVoucherHelper.GetBCTcode(accIds, ApiTask);

            // 将对象一一赋值给另一个对象，避免企业凭证的条件会到银行对账单
            filterParm.ForEachKeys(x =>
            {
                corpVDfilterParm.Append(x, filterParm[x]);
                bankVDfilterParm.Append(x, filterParm[x]);
            });

            // * 分银行对账进行自动勾对
            foreach (var banka in bankAList)
            {
                var bkaccountId = banka.Item<int>("bkaccount.id");

                // 企业凭证-添加过滤条件
                corpVDfilterParm.Append("subunit", $"[{banka["subunit.id"]}]");
                corpVDfilterParm.Append("account", $"[{bkaccountId}]");
                corpVDfilterParm.Append("vh.vtype", $"!{AcConst.BankVhType}"); // 过滤掉凭证类型为z0.对账单的银行对账单

                // 银行对账单-添加过滤条件
                bankVDfilterParm.Append("subunit", $"[{banka["subunit.id"]}]");
                bankVDfilterParm.Append("bkaccount", $"[{bkaccountId}]");
                bankVDfilterParm.Append("bank", $"[{banka["bank.id"]}]");
                bankVDfilterParm.Append("vh.vtype", AcConst.BankVhType); // 筛选凭证类型为z0.对账单的银行对账单
                bankVDfilterParm.Append("rstate", AcConst.NeverTick); // 银行对账单从未勾对

                // 凭证分录返回的列
                string columns = "id,cd,bdate,reference,account.id,amountc,amountd,SubUnit,vstate,vclass";

                // 银行科目启用了银行账户Tcode，则需要银行账户来过滤企业凭证
                var tcode = tcodeData.Item<string>(bkaccountId.ToString());
                if (!string.IsNullOrEmpty(tcode))
                {
                    corpVDfilterParm.Append(tcode, $"[{banka["bank.id"]}]");
                    columns += "," + tcode;
                }

                // * 按业务参考优先勾对
                AutoRecoByReference(qParm, corpVDfilterParm, bankVDfilterParm, ref recoCount, ref recoAmountSum);

                // * 按业务日期小的优先勾对
                // * 企业凭证-按业务日期分组查询企业凭证
                var bdateList = GetVoucherD(qParm, corpVDfilterParm, string.Empty, 0);
                if (bdateList.Count == 0)
                {
                    continue;
                }

                // * 循环业务日期
                foreach (var bdate in bdateList)
                {
                    // 企业凭证和对账单进行勾对
                    AutoRecoByBdate(bdate, qParm, corpVDfilterParm, bankVDfilterParm, columns, ref recoCount, ref recoAmountSum);
                }
            }

            if (recoCount == 0)
            {
                throw new Exception(ApiTask.LEnd("无未勾对的数据"));
            }

            return $"自动勾对完成，勾对{recoCount}条数据，勾对金额{recoAmountSum}元";
        }

        /// <summary>
        /// 自动勾对-按业务日期升序勾对
        /// 1、变量声明
        /// 2、获取分页过滤条件
        /// 3、根据筛选条件获取符合要求的企业凭证数据
        /// 4、遍历企业凭证
        /// 5、筛选出(科目相同 、金额相同、分析项且分析项值相同)的银行对账单（按业务日期排序取第一个）
        /// 6、修改企业凭证和银行对账的勾对数据
        /// </summary>
        /// <param name="bdate">业务日期</param>
        /// <param name="qParm">查询条件</param>
        /// <param name="corpVDfilterParm">企业凭证查询条件</param>
        /// <param name="bankVDfilterParm">银行对账单查询条件</param>
        /// <param name="columns">企业凭证列</param>
        /// <param name="recoCount">勾对体成功添加的条数</param>
        /// <param name="recoAmountSum">总的勾对金额</param>
        private void AutoRecoByBdate(SData bdate, SData qParm, SData corpVDfilterParm, SData bankVDfilterParm, string columns, ref int recoCount, ref decimal recoAmountSum)
        {
            // * 变量声明
            int voucherdIdMax = 0; // 保存每次获取记录最大凭证Id
            int voucherdCount = 0; // 符合要求的凭证分录数
            string loginUser = ApiTask.UserInfo().UserCode();
            var bizBankVd = ApiTask.Biz<BizTable>("AcBankVD");
            var maxbDate = bdate.Item<string>("bdate");

            do
            {
                try
                {
                    // * 获取分页过滤条件
                    qParm.Append("bdate", $"{maxbDate}");
                    qParm.Append("voucherdid", $"!:{voucherdIdMax}");

                    // * 根据筛选条件获取符合要求的企业凭证数据
                    List<SData> corpList = GetVoucherD(qParm, corpVDfilterParm, columns, AcConst.RecoPageSize);
                    voucherdCount = corpList.Count;

                    if (voucherdCount > 0)
                    {
                        // 获取最后一条凭证，记录最大id
                        var lastVoucherdItem = corpList.LastOrDefault();
                        voucherdIdMax = lastVoucherdItem.Item<int>("id");
                    }

                    // * 遍历企业凭证
                    foreach (var corpVD in corpList)
                    {
                        decimal amountValue = 0;

                        // 清查询参数
                        bankVDfilterParm.Remove("amountc");
                        bankVDfilterParm.Remove("amountd");

                        // 查询银行对账单条件(科目相同、Tcode相同、金额相同、方向相同)=》金额相同、方向相同
                        var cd = corpVD.Item<bool>("cd");
                        if (!cd)
                        {
                            amountValue = corpVD.Item<decimal>("amountd");
                            bankVDfilterParm.Append("amountd", $"[{amountValue}]"); // todo
                        }
                        else
                        {
                            amountValue = corpVD.Item<decimal>("amountc");
                            bankVDfilterParm.Append("amountc", $"[{amountValue}]"); // todo
                        }

                        // * 筛选出(科目相同 、金额相同、分析项且分析项值相同)的银行对账单（按业务日期排序取第一个）
                        var bankVD = bizBankVd.GetListData(1, 0, bankVDfilterParm.toParmStr(), "id").FirstOrDefault();

                        if (bankVD?.Count > 0)
                        {
                            try
                            {
                                // * 修改企业凭证的勾对金额、勾对状态、勾对日期
                                // 修改银行对账单的勾对金额、勾对状态、勾对日期、勾对类型、勾对人
                                var recoDId = AcVoucherHelper.UpdateVoucherD(this, amountValue, AcConst.RecoAuto, loginUser, corpVD.Item<int>("id"), bankVD.Item<int>("id"));
                                if (recoDId > 0)
                                {
                                    recoCount += 2;

                                    // 勾对金额(企业凭证：借-贷)
                                    if (!cd)
                                    {
                                        recoAmountSum = recoAmountSum + amountValue;
                                    }
                                    else
                                    {
                                        recoAmountSum = recoAmountSum - amountValue;
                                    }
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
            } while (voucherdCount == AcConst.RecoPageSize);
        }

        /// <summary>
        /// 查询出满足条件的凭证分录
        /// </summary>
        /// <param name="queryParam">更新后的查询条件</param>
        /// <param name="filterParameters">满足条件的科目</param>
        /// <param name="columns">企业凭证列</param>
        /// <param name="pageSize">页码</param>
        /// <returns>满足条件的凭证分录</returns>
        private List<SData> GetVoucherD(SData queryParam, SData filterParameters, string columns, int pageSize)
        {
            // 添加业务日期条件
            filterParameters.Append("bdate", queryParam.Item<string>("bdate"));

            // 排除勾对过的凭证分录
            var voucherdid = queryParam.Item<string>("voucherdid");
            if (!string.IsNullOrEmpty(voucherdid))
            {
                filterParameters.Append("id", voucherdid);
            }

            var voucherDData = new List<SData>();
            var biz = ApiTask.Biz<BizTable>("AcCorpVD");

            if (pageSize > 0)
            {
                // * 获取凭证分录数据
                voucherDData = this.GetListData(pageSize, 0, filterParameters.toParmStr(), columns);
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
        /// 自动勾对-优先业务参考勾对
        /// 1、变量声明
        /// 2、获取过滤条件
        /// 3、根据筛选条件获取符合要求的银行对账单数据
        /// 4、遍历银行对账单
        /// 5、筛选出(业务参考相同、科目相同 、金额相同、分析项且分析项值相同)的企业凭证（按业务日期排序取第一个）
        /// 6、修改企业凭证和银行对账的勾对数据
        /// </summary>
        /// <param name="qParm">查询条件</param>
        /// <param name="corpVDfilterParm">企业凭证查询条件</param>
        /// <param name="bankVDfilterParm">银行对账单查询条件</param>
        /// <param name="recoCount">勾对体成功添加的条数</param>
        /// <param name="recoAmountSum">总的勾对金额</param>
        private void AutoRecoByReference(SData qParm, SData corpVDfilterParm, SData bankVDfilterParm, ref int recoCount, ref decimal recoAmountSum)
        {
            // *变量声明
            int voucherdIdMax = 0; // 保存每次获取记录最大凭证Id
            int voucherdCount = 0; // 符合要求的凭证分录数
            string loginUser = ApiTask.UserInfo().UserCode();

            // * 过滤条件
            var newQParm = new SData();
            qParm.ForEachKeys(x =>
            {
                newQParm.Append(x, qParm[x]);
            });
            var newCorpParm = new SData();
            corpVDfilterParm.ForEachKeys(x =>
            {
                newCorpParm.Append(x, corpVDfilterParm[x]);
            });

            var newBankParm = new SData();
            bankVDfilterParm.ForEachKeys(x =>
            {
                newBankParm.Append(x, bankVDfilterParm[x]);
            });
            newBankParm.Append("reference", "(1)");
            newBankParm.Append("q.orderby", "id"); // 按id升序排列

            // 分每页100条取银行对账单数据
            do
            {
                try
                {
                    // 用最大Id来区分页码
                    newQParm.Append("voucherdid", $"!:{voucherdIdMax}");

                    // * 根据筛选条件获取符合要求的银行对账单数据
                    List<SData> bankList = GetBankVD(newQParm, newBankParm, AcConst.RecoPageSize);
                    voucherdCount = bankList.Count;

                    if (voucherdCount > 0)
                    {
                        // 获取最后一条凭证，记录最大id
                        var lastVoucherdItem = bankList.LastOrDefault();
                        voucherdIdMax = lastVoucherdItem.Item<int>("id");
                    }

                    // * 遍历银行对账单
                    foreach (var bankVd in bankList)
                    {
                        decimal amountValue = 0;

                        // 清查询参数
                        newCorpParm.Remove("amountc");
                        newCorpParm.Remove("amountd");
                        newCorpParm.Remove("reference");

                        // 查询企业凭证条件(科目相同、Tcode相同、金额相同、方向相同)=》金额相同、方向相同
                        var cd = bankVd.Item<bool>("cd");
                        if (!cd)
                        {
                            amountValue = bankVd.Item<decimal>("amountd");
                            newCorpParm.Append("amountd", $"[{amountValue}]"); // todo
                        }
                        else
                        {
                            amountValue = bankVd.Item<decimal>("amountc");
                            newCorpParm.Append("amountc", $"[{amountValue}]"); // todo
                        }

                        // * 筛选出(业务参考相同、科目相同 、金额相同、分析项且分析项值相同)的企业凭证（按业务日期排序取第一个）
                        newCorpParm.Append("reference", bankVd.Item<string>("reference"));

                        var corpVd = this.GetListData(1, 0, newCorpParm.toParmStr(), "id").FirstOrDefault();

                        if (corpVd?.Count > 0)
                        {
                            try
                            {
                                // * 修改企业凭证的勾对金额、勾对状态、勾对日期
                                // 修改银行对账单的勾对金额、勾对状态、勾对日期、勾对类型、勾对人
                                var recoDId = AcVoucherHelper.UpdateVoucherD(this, amountValue, AcConst.RecoAuto, loginUser, corpVd.Item<int>("id"), bankVd.Item<int>("id"));
                                if (recoDId > 0)
                                {
                                    recoCount += 2;

                                    // 勾对金额(对账单：借-贷)
                                    if (!cd) // todo
                                    {
                                        recoAmountSum = recoAmountSum + amountValue;
                                    }
                                    else
                                    {
                                        recoAmountSum = recoAmountSum - amountValue;
                                    }
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
            } while (voucherdCount == AcConst.RecoPageSize);
        }

        /// <summary>
        /// 查询出满足条件的银行对账单分录
        /// </summary>
        /// <param name="queryParam">更新后的查询条件</param>
        /// <param name="filterParameters">满足条件的科目</param>
        /// <param name="pageSize">页码</param>
        /// <returns>满足条件的银行对账单分录</returns>
        private List<SData> GetBankVD(SData queryParam, SData filterParameters, int pageSize)
        {
            // 分页排除勾对过的凭证分录
            var voucherdid = queryParam.Item<string>("voucherdid");
            if (!string.IsNullOrEmpty(voucherdid))
            {
                filterParameters.Append("id", voucherdid);
            }

            var biz = ApiTask.Biz<BizTable>("AcBankVD");

            // 银行对账单返回的列
            string columns = "id,cd,bdate,reference,bkaccount.id,amountc,amountd,SubUnit,vstate,vclass,bank";

            // * 获取银行对账单分录数据
            var voucherDData = biz.GetListData(pageSize, 0, filterParameters.toParmStr(), columns);

            return voucherDData;
        }
        #endregion

        #region 手动勾对

        /// <summary>
        /// 手动勾对
        /// 1、勾对数据判空校验
        /// 2、银行对账判空校验
        /// 3、核算单位判断校验
        /// 4、勾对数据金额校验
        /// 5、勾对数据逻辑验证
        /// </summary>
        /// <param name="qParm">查询及勾对数据</param>
        private void ManualReco(SData qParm)
        {
            // * 勾对数据判空校验
            var recod = qParm.Item<string>("recod");
            if (string.IsNullOrEmpty(recod))
            {
                throw new Exception(ApiTask.LEnd("无勾对的数据"));
            }

            // 将json转换成List<SData>
            var recodList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SData>>(recod);

            if (recodList == null || recodList.Count == 0)
            {
                throw new Exception(ApiTask.LEnd("无勾对的数据"));
            }

            var corpRecoList = recodList.Where(x => x["isBank"] != null && x["isBank"].Equals(0)).ToList();
            var bankRecoList = recodList.Where(x => x["isBank"] != null && x["isBank"].Equals(1)).ToList();
            if (corpRecoList.Count == 0 && bankRecoList.Count == 0)
            {
                throw new Exception(ApiTask.LEnd("无勾对的数据"));
            }

            if (corpRecoList.Count > 0 && bankRecoList.Count == 0)
            {
                throw new Exception(ApiTask.LEnd("无勾对的银行对账单数据"));
            }

            if (corpRecoList.Count == 0 && bankRecoList.Count > 0)
            {
                throw new Exception(ApiTask.LEnd("无勾对的企业银行账数据"));
            }

            // * 银行对账判空校验
            var banka = qParm.Item<int>("banka");
            if (banka <= 0)
            {
                throw new Exception(ApiTask.LEnd("银行对账不存在"));
            }

            var bankAItem = ApiTask.Biz<BizTable>("AcBankA").GetItem(banka, "id,subunit.id,subunit.dcode,bkaccount.id,bank.id");
            if (bankAItem == null || bankAItem.Count < 0)
            {
                throw new Exception(ApiTask.LEnd("未设置银行对账，不允许勾对"));
            }

            if (string.IsNullOrEmpty(bankAItem.Item<string>("bkaccount.id")))
            {
                throw new Exception(ApiTask.LEnd("银行科目不存在"));
            }

            // * 核算单位判断校验
            // 当前核算单位
            string currentSubunit = ApiTask.UserInfo().UserSubUnit();
            var subunitCode = bankAItem.Item<string>("subunit.dcode");

            // 登录用户的核算单位与条件传入的核算单位不一致
            if (!string.IsNullOrEmpty(currentSubunit) && !currentSubunit.Sp_First(".").Equals(subunitCode))
            {
                throw new Exception(ApiTask.LEnd("核算单位不一致，不允许勾对"));
            }

            // * 勾对数据金额校验
            foreach (var recoD in recodList)
            {
                // 勾对金额校验（不能为空，不能为0，是数值）
                AcVoucherHelper.CheckDecimal(this.ApiTask, recoD.Item<string>("recoamount"), ((FieldDefine)ListField["recoamount"]).DisplayName);
            }

            // * 勾对数据逻辑验证
            AddRecoDBusinessLogic(corpRecoList, bankRecoList, bankAItem);
        }

        /// <summary>
        /// 手动勾对逻辑校验及勾对
        /// 1、校验企业账勾对数据
        /// 2、校验银行账勾对数据
        /// 3、企业银行账金额合计不等于银行对账单金额合计
        /// 4、拆分部分勾对的银行对账单（待勾对的银行对账单集合和未勾对的银行对账单集合）
        /// 5、根据勾对规则勾对企业凭证和银行对账单
        /// 6、修改企业凭证的勾对金额、勾对状态、勾对日期
        /// 7、修改银行对账单的勾对金额、勾对状态、勾对日期、勾对类型、勾对人
        /// 8、删除拆分前的银行对账单
        /// 9、新增拆分的银行对账单
        /// </summary>
        /// <param name="corpRecoList">企业凭证勾对集合</param>
        /// <param name="bankRecoList">银行对账单勾对集合</param>
        /// <param name="bankAItem">银行对账</param>
        private void AddRecoDBusinessLogic(List<SData> corpRecoList, List<SData> bankRecoList, SData bankAItem)
        {
            // * 校验企业账勾对数据
            var corpVDList = new List<SData>();
            decimal corpRecoamountSum = CheckRecoData(corpRecoList, bankAItem, true, ref corpVDList);

            // * 校验银行账勾对数据
            var bankVDList = new List<SData>();
            decimal bankRecoamountSum = CheckRecoData(bankRecoList, bankAItem, false, ref bankVDList);

            // * 企业银行账金额合计不等于银行对账单金额合计
            if (corpRecoamountSum != bankRecoamountSum)
            {
                throw new Exception(ApiTask.LEnd("所选记录借贷不平衡，不允许勾对"));
            }

            // * 拆分部分勾对的银行对账单（待勾对的银行对账单集合和未勾对的银行对账单集合）
            var delBankVDList = new List<SData>(); // 需要删除的原银行对账单集合
            var bankVDYesList = new List<SData>(); // 待勾对的银行对账单集合
            var bankVDNoList = SpiltBankVD(bankVDList, ref bankVDYesList, ref delBankVDList);

            // * 根据勾对规则勾对企业凭证和银行对账单
            var updateBankVDList = new List<SData>();

            // 第一优先级：业务参考相同，勾对金额相等，方向相同
            FirstReco(corpVDList, bankVDYesList, ref updateBankVDList);

            // 第二优先级：业务参考相同，方向相同
            SecondReco(corpVDList, bankVDYesList, updateBankVDList, delBankVDList);

            // 第三优先级：勾对金额相等，方向相同
            ThirdReco(corpVDList, bankVDYesList, updateBankVDList);

            // 第四优先级：业务日期升序，方向相同
            FourReco(corpVDList, bankVDYesList, updateBankVDList, delBankVDList);

            // * 修改企业凭证的勾对金额、勾对状态、勾对日期
            // 筛选出本次需要勾对的企业凭证集合
            var updateCorpVDList = corpVDList.Where(x => x.Item<decimal>("thisrecoamount") != 0);

            // 拼接sql修改企业凭证
            string sql = string.Empty;
            string loginUser = ApiTask.UserInfo().UserCode();
            var trialId = ConstResourceDB.GetId(this.ApiTask, nameof(AcVstate), AcConst.Trial); // 记账状态下子状态可以为正常或锁定
            var rstateId = ConstResourceDB.GetId(this.ApiTask, nameof(AcVSubstate), AcConst.LockS);
            foreach (var corp in updateCorpVDList)
            {
                var id = corp.Item<int>("id");
                var recoAmount = corp.Item<decimal>("thisrecoamount");

                sql += $@"
update m 
set a9=(select id from others where dclass='acrecostate' and dcode=(case when (isnull(m.d0,0)+{recoAmount})!=(m.amountd+m.amountc) then '{AcConst.PartTick}' else '{AcConst.AllTick}' end)),
d0=(isnull(m.d0,0)+{recoAmount}),
a8=(select id from others where dclass='acrecotype' and dcode='{AcConst.RecoManual}'),
a7=(select id from users where dclass='user' and dcode='{loginUser}'),
s0=(case when m.id={id} then '{AcConst.BigDate}' else '{DateTime.Now.ToString("yyyy-MM-dd")}' end),
a4={id},
vsubstate=(case when m.vstate={trialId} then {rstateId} else m.vsubstate end) 
from 
voucherd as m 
where 
m.id={id};
update m 
set vsubstate=(case when m.vstate={trialId} then {rstateId} else m.vsubstate end) 
from 
voucher as m 
where 
exists(select vh from voucherd where id={id} and vh=m.id);";
            }

            // * 修改银行对账单的勾对金额、勾对状态、勾对日期、勾对类型、勾对人
            var uptBankVDList = updateBankVDList.Where(x => x.Item<int>("isInsert") != 1).ToList();
            foreach (var bank in uptBankVDList)
            {
                var id = bank.Item<int>("id");
                var recoAmount = bank.Item<decimal>("newrecoamount");
                var bparent = bank.Item<int>("bparent");

                sql += $@"
update m 
set a9=(select id from others where dclass='acrecostate' and dcode='{AcConst.AllTick}'),
d0=(isnull(m.d0,0)+{recoAmount}),
a8=(select id from others where dclass='acrecotype' and dcode='{AcConst.RecoManual}'),
a7=(select id from users where dclass='user' and dcode='{loginUser}'),
s0='{DateTime.Now.ToString("yyyy-MM-dd")}',
a4={bparent} 
from 
voucherd as m 
where m.id={id};";
            }

            // 添加事务
            this.ApiTask.DB.BeginTransaction();

            // * 新增拆分后勾对成功的银行对账单
            var insertBankVDList = updateBankVDList.Where(x => x.Item<int>("isInsert") == 1).ToList();
            AddBankVDList(insertBankVDList, loginUser);

            var insertIdList = insertBankVDList.Select(x => x.Item<int>("id")).Distinct().ToList();

            // 新增部分勾对拆分后不需要勾对的银行对账单和新增拆分后未勾对成功的银行对账单
            InsertBankVdNoReco(insertIdList, bankVDNoList, bankVDYesList, loginUser);

            // * 删除拆分前的银行对账单-insertBankVDList中包含
            delBankVDList.ForEach(d =>
            {
                var id = d.Item<int>("id");
                if (insertIdList.Contains(id))
                {
                    var vh = d.Item<int>("vh.id");
                    if (id > 0)
                    {
                        sql += $@"
delete from voucherd where id = '{id}';
delete from voucher where id = '{vh}';";
                    }
                }
            });

            // 新增对账单不报错后执行sql
            if (!string.IsNullOrEmpty(sql))
            {
                this.ApiTask.DB.ExecuteNonQuery(sql);
            }
            else
            {
                throw new Exception(ApiTask.LEnd("无勾对的数据"));
            }

            this.ApiTask.DB.Commit();
        }

        /// <summary>
        /// 校验企业账或银行账勾对数据
        /// 1、获取银行科目启用的银行账户tcode
        /// 2、企业凭证/银行对账单要筛选的字段
        /// 3、获取企业凭证/银行对账单数据集合
        /// 4、遍历需要添加的企业凭证/银行对账单勾对数据
        /// 5、校验凭证分录/对账单不存在
        /// 6、校验凭证分录未记账
        /// 7、校验凭证分录/对账单的核算单位不相同
        /// 8、校验凭证分录/对账单的银行科目不相同
        /// 9、校验凭证分录/对账单的银行账户不相同
        /// 10、校验凭证分录/对账单的勾对金额大于未勾对的金额
        /// 11、计算企业账/银行账勾对金额合计
        /// 12、将勾对金额放到凭证或银行对账单集合中
        /// </summary>
        /// <param name="recoList">企业账或银行账</param>
        /// <param name="bankAItem">银行对账</param>
        /// <param name="isCorp">true企业账，false银行账</param>
        /// <param name="vdList">返回企业账或银行账数据</param>
        /// <returns>返回企业账或银行账勾对金额合计</returns>
        private decimal CheckRecoData(List<SData> recoList, SData bankAItem, bool isCorp, ref List<SData> vdList)
        {
            // * 获取银行科目启用的银行账户tcode
            var bkaccountId = bankAItem.Item<string>("bkaccount.id");
            var tcodeData = AcVoucherHelper.GetBCTcode(bkaccountId, ApiTask);
            var tcode = tcodeData.Item<string>(bkaccountId);

            // * 企业凭证/银行对账单要筛选的字段
            var columns = "id,bdate,reference,vstate,cd,amountd,amountc,subunit.id,recoamount,period,ddesc,vh.id,vclass.dcode";
            if (isCorp)
            {
                // 企业凭证要筛选的字段
                columns += $",account.id";

                if (!string.IsNullOrEmpty(tcode))
                {
                    columns += $",{tcode}.id";
                }
            }
            else
            {
                // 银行对账单要筛选的字段
                columns += $",bkaccount.id,bank.id";
            }

            // * 获取企业凭证/银行对账单数据集合
            // 凭证Id/对账单Id
            var pIdList = recoList.Select(x => x["id"].ToString()).ToList();

            // 分300/页 获取企业凭证id/对账单Id
            var vdPIds = AcVoucherHelper.GetIdsByPage(pIdList);
            foreach (var id in vdPIds.Keys)
            {
                var ids = string.Join(",", vdPIds.Item<List<string>>(id).ToList());

                // 获取企业凭证/银行对账单数据集合(按业务日期升序排序)
                var parms = new SData("id", ids);
                parms.Append("q.orderby", "bdate,id");
                var biz = isCorp ? this : ApiTask.Biz<BizTable>("AcBankVD");
                var list = biz.GetListData(0, 1, parms.toParmStr(), columns);
                vdList.AddRange(list);
            }

            decimal recoamountSum = 0;  // 勾对金额合计

            // * 遍历需要添加的企业凭证/银行对账单勾对数据
            foreach (var recoD in recoList)
            {
                int id = int.Parse(recoD["id"].ToString()); // 凭证Id

                // * 凭证分录/对账单不存在，提示：所选记录不存在
                var vd = vdList.FirstOrDefault(x => x["id"].ToString().ToInt() == id);
                if (vd == null)
                {
                    throw new Exception(ApiTask.LEnd("所选记录不存在"));
                }

                // * 凭证分录不是实际数据，提示：所选记录不是实际数据，不允许勾对
                // getlist中默认了实际数据查询，异常抛不出来
                if (isCorp && !vd["vclass.dcode"].ToString().StartsWith(AcConst.ActData))
                {
                    throw new Exception(ApiTask.LEnd("所选记录不是实际数据，不允许勾对"));
                }

                // * 凭证分录未记账，提示：所选记录未记账，不允许勾对
                // getlist中默认了记账数据查询，异常抛不出来
                if (isCorp && !vd.Item<string>("vstate").StartsWith(AcConst.Trial))
                {
                    throw new Exception(ApiTask.LEnd("所选记录未记账，不允许勾对"));
                }

                // * 凭证分录/对账单的核算单位不相同，提示：所选记录核算单位不相同，不允许勾对
                if (vd.Item<int>("subunit.id") != bankAItem.Item<int>("subunit.id"))
                {
                    throw new Exception(ApiTask.LEnd("所选记录核算单位不相同，不允许勾对"));
                }

                // * 凭证分录/对账单的银行科目不相同，提示：所选记录银行科目不相同，不允许勾对
                var accId = vd.Item<int>(isCorp ? "account.id" : "bkaccount.id");

                if (accId != bankAItem.Item<int>("bkaccount.id"))
                {
                    throw new Exception(ApiTask.LEnd("所选记录银行科目不相同，不允许勾对"));
                }

                // * 凭证分录/对账单的银行账户不相同，提示：所选记录银行账户不相同，不允许勾对
                // 银行科目启用tcode银行账户
                if (isCorp)
                {
                    if (!string.IsNullOrEmpty(tcode))
                    {
                        if (vd.Item<int>($"{tcode}.id") != bankAItem.Item<int>("bank.id"))
                        {
                            throw new Exception(ApiTask.LEnd("所选记录银行账户不相同，不允许勾对"));
                        }
                    }
                }
                else
                {
                    if (vd.Item<int>("bank.id") != bankAItem.Item<int>("bank.id"))
                    {
                        throw new Exception(ApiTask.LEnd("所选记录银行账户不相同，不允许勾对"));
                    }
                }

                // * 凭证分录/对账单的勾对金额大于未勾对的金额，提示：所选记录勾对金额大于未勾对金额，不允许勾对
                var oldRecoAmount = vd.Item<decimal>("recoamount");
                decimal recoamount = recoD.Item<decimal>("recoamount");

                // 凭证分录未勾对金额 (未勾对金额：借方金额 + 贷方金额 - 已勾金额)
                decimal noRecoAmount = vd.Item<decimal>("amountd") + vd.Item<decimal>("amountc") - oldRecoAmount;

                // 未勾对金额为负数
                if (noRecoAmount < 0)
                {
                    if (Math.Abs(recoamount) > Math.Abs(noRecoAmount))
                    {
                        throw new Exception(ApiTask.LEnd("所选记录勾对金额大于未勾对金额，不允许勾对"));
                    }
                }
                else
                {
                    if (recoamount > noRecoAmount)
                    {
                        throw new Exception(ApiTask.LEnd("所选记录勾对金额大于未勾对金额，不允许勾对"));
                    }
                }

                // * 企业账/银行账勾对金额合计
                if (recoD["cd"].ToString().ToInt() == AcConst.Debit)
                {
                    recoamountSum = recoamountSum + recoamount;
                }
                else
                {
                    recoamountSum = recoamountSum - recoamount;
                }

                // * 将勾对金额放到凭证或银行对账单集合中
                vd.Append("newrecoamount", recoamount);
            }

            return recoamountSum;
        }

        /// <summary>
        /// 企业凭证和银行对账单勾对(第一优先级：业务参考相同，勾对金额相等，方向相同)
        /// 1、企业凭证集合循环
        /// 2、筛选银行对账单
        /// 3、待修改到DB的银行对账单
        /// 4、待修改到DB的企业凭证
        /// 5、银行对账单集合移除该条银行对账单（剩余银行对账单集合）
        /// </summary>
        /// <param name="corpVDList">企业凭证集合</param>
        /// <param name="bankVDList">银行对账单集合</param>
        /// <param name="updateBankVDList">返回待修改到DB的银行对账单</param>
        private void FirstReco(List<SData> corpVDList, List<SData> bankVDList, ref List<SData> updateBankVDList)
        {
            var newCorpVDList = corpVDList.Where(x => !string.IsNullOrEmpty(x.Item<string>("reference")) && x.Item<decimal>("newrecoamount") != 0).OrderBy(x => x.Item<string>("bdate")).ToList();
            var newBankVDList = bankVDList.Where(x => !string.IsNullOrEmpty(x.Item<string>("reference"))).OrderBy(x => x.Item<string>("bdate")).ToList();

            if (newCorpVDList.Count > 0 && newBankVDList.Count > 0)
            {
                // * 企业凭证集合循环
                foreach (var corp in newCorpVDList)
                {
                    // * 筛选银行对账单，第一优先级：业务参考相同，勾对金额相等，方向相同
                    var reference = corp.Item<string>("reference");
                    var recoamount = corp.Item<decimal>("newrecoamount");
                    var cd = corp.Item<bool>("cd");
                    var bankVd = newBankVDList.FirstOrDefault(x => x.Item<string>("reference") == reference &&
                                                                   x.Item<decimal>("newrecoamount") == recoamount &&
                                                                   x.Item<bool>("cd") == cd);
                    if (bankVd?.Count > 0)
                    {
                        // * 待修改到DB的银行对账单
                        var updateBankVD = new SData();
                        bankVd.ForEachKeys(x => { updateBankVD.Append(x, bankVd[x]); });
                        updateBankVD.Append("bparent", corp["id"]);
                        updateBankVDList.Add(updateBankVD);

                        // * 待修改到DB的企业凭证
                        corp.Append("thisrecoamount", recoamount); // 本次勾对金额
                        corp.Append("newrecoamount", 0); // 本次剩余未勾对金额

                        // * 银行对账单集合移除该条银行对账单（剩余银行对账单集合）
                        newBankVDList.Remove(bankVd);
                        bankVDList.Remove(bankVd);
                    }
                }
            }
        }

        /// <summary>
        /// 企业凭证和银行对账单勾对(第二优先级：业务参考相同，方向相同)
        /// 1、企业凭证集合循环
        /// 2、筛选银行对账单
        /// 3、待修改到DB的银行对账单
        /// 4、银行对账单集合移除该条银行对账单（剩余银行对账单集合）
        /// 5、企业凭证集合移除该条企业凭证（剩余企业凭证集合）
        /// </summary>
        /// <param name="corpVDList">企业凭证</param>
        /// <param name="bankVDList">银行对账单</param>
        /// <param name="updateBankVDList">返回待修改到DB的银行对账单</param>
        /// <param name="delBankVDList">需要删除的原银行对账单集合</param>
        private void SecondReco(List<SData> corpVDList, List<SData> bankVDList, List<SData> updateBankVDList, List<SData> delBankVDList)
        {
            var newCorpVDList = corpVDList.Where(x => !string.IsNullOrEmpty(x.Item<string>("reference")) && x.Item<decimal>("newrecoamount") != 0).OrderBy(x => x.Item<string>("bdate")).ToList();
            var newBankVDList = bankVDList.Where(x => !string.IsNullOrEmpty(x.Item<string>("reference"))).ToList();

            if (newCorpVDList.Count > 0 && newBankVDList.Count > 0)
            {
                // * 企业凭证集合循环
                foreach (var corp in newCorpVDList)
                {
                    // * 筛选银行对账单，第二优先级：业务参考相同，方向相同
                    var reference = corp.Item<string>("reference");
                    var recoamount = corp.Item<decimal>("newrecoamount");
                    var cd = corp.Item<bool>("cd");

                    // newBankVDList有添加需要每次查询时排序
                    var bankVDRList = newBankVDList.Where(x => x.Item<string>("reference") == reference && x.Item<bool>("cd") == cd).OrderBy(x => x.Item<string>("bdate")).ToList();

                    if (bankVDRList.Count > 0)
                    {
                        // 对账单勾对金额金额合计
                        var recoamountSum = bankVDRList.Sum(x => x.Item<decimal>("newrecoamount"));
                        var thisrecoamount = corp.Item<decimal>("thisrecoamount");

                        // * 凭证金额与对账单金额合计比较
                        if (recoamount == recoamountSum) // 等于
                        {
                            bankVDRList.ForEach(b =>
                            {
                                // * 待修改到DB的银行对账单
                                var updateBankVD = new SData();
                                b.ForEachKeys(x => { updateBankVD.Append(x, b[x]); });
                                updateBankVD.Append("bparent", corp["id"]);
                                updateBankVDList.Add(updateBankVD);

                                // * 银行对账单集合移除该条银行对账单（剩余银行对账单集合）
                                newBankVDList.Remove(b);
                                bankVDList.Remove(b);
                            });

                            // * 待修改到DB的企业凭证
                            corp.Append("thisrecoamount", thisrecoamount + recoamount); // 本次勾对金额
                            corp.Append("newrecoamount", 0); // 本次剩余未勾对金额
                        }
                        else if (recoamount > recoamountSum) // 大于
                        {
                            bankVDRList.ForEach(b =>
                            {
                                // * 待修改到DB的银行对账单
                                var updateBankVD = new SData();
                                b.ForEachKeys(x => { updateBankVD.Append(x, b[x]); });
                                updateBankVD.Append("bparent", corp["id"]);
                                updateBankVDList.Add(updateBankVD);

                                // * 银行对账单集合移除该条银行对账单（剩余银行对账单集合）
                                newBankVDList.Remove(b);
                                bankVDList.Remove(b);
                            });

                            // * 企业凭证集合中修改该条企业凭证勾对金额（剩余企业凭证集合）
                            corp.Append("thisrecoamount", recoamountSum + thisrecoamount); // 本次勾对金额
                            corp.Append("newrecoamount", (recoamount - recoamountSum).ToString().ToDec()); // 本次剩余勾对金额
                        }
                        else // 小于
                        {
                            decimal bkrecoamount = 0;

                            foreach (var b in bankVDRList)
                            {
                                bkrecoamount += b.Item<decimal>("newrecoamount");
                                decimal bkamount = recoamount - bkrecoamount; // 银行对账单未勾对金额
                                if (bkamount >= 0)
                                {
                                    // * 待修改到DB的银行对账单
                                    var updateBankVD = new SData();
                                    b.ForEachKeys(x => { updateBankVD.Append(x, b[x]); });
                                    updateBankVD.Append("bparent", corp["id"]);
                                    updateBankVDList.Add(updateBankVD);

                                    // * 银行对账单集合移除该条银行对账单（剩余银行对账单集合）
                                    newBankVDList.Remove(b);
                                    bankVDList.Remove(b);
                                }
                                else
                                {
                                    // * 拆分银行对账单（删除原对账单，添加拆分后的已勾对对账单，）
                                    // ---该批中某条银行对账单拆分出未勾对的数据
                                    var yes = new SData();
                                    var no = new SData();
                                    b.ForEachKeys(x =>
                                    {
                                        yes.Append(x, b[x]);
                                        no.Append(x, b[x]);
                                    });
                                    SpiltBankVD(b, bkamount, ref yes, ref no);

                                    // ---银行对账单集合添加拆分后的银行对账单
                                    // 银行对账单集合添加拆分后未勾对玩的对账单
                                    newBankVDList.Add(no);
                                    bankVDList.Add(no);

                                    // * 待新增到DB的银行对账单
                                    yes.Append("bparent", corp["id"]);
                                    updateBankVDList.Add(yes);

                                    // 原拆单b是待新增到DB的银行对账单，那么需要移除b
                                    if (b.Item<int>("isinsert") == 1)
                                    {
                                        updateBankVDList.Remove(b);
                                    }
                                    else
                                    {
                                        // 删除原对账单
                                        delBankVDList.Add(b);
                                    }

                                    // * 银行对账单集合移除该条银行对账单（剩余银行对账单集合）
                                    newBankVDList.Remove(b);
                                    bankVDList.Remove(b);

                                    break;
                                }

                                // 勾对完时，跳出循环
                                if (bkamount == 0)
                                {
                                    break;
                                }
                            }

                            // * 待修改到DB的企业凭证
                            corp.Append("thisrecoamount", thisrecoamount + recoamount); // 本次勾对金额
                            corp.Append("newrecoamount", 0); // 本次剩余未勾对金额
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 企业凭证和银行对账单勾对(第三优先级：勾对金额相等，方向相同)
        /// 1、企业凭证集合循环
        /// 2、筛选银行对账单
        /// 3、待修改到DB的银行对账单
        /// 4、待修改到DB的企业凭证
        /// 5、银行对账单集合移除该条银行对账单（剩余银行对账单集合）
        /// 6、企业凭证集合移除该条企业凭证（剩余企业凭证集合）
        /// </summary>
        /// <param name="corpVDList">企业凭证</param>
        /// <param name="bankVDList">银行对账单</param>
        /// <param name="updateBankVDList">返回待修改到DB的银行对账单</param>
        private void ThirdReco(List<SData> corpVDList, List<SData> bankVDList, List<SData> updateBankVDList)
        {
            var newCorpVDList = corpVDList.Where(x => x.Item<decimal>("newrecoamount") != 0).OrderBy(x => x.Item<string>("bdate")).ToList();
            var newBankVDList = bankVDList.OrderBy(x => x.Item<string>("bdate")).ToList();
            if (newCorpVDList.Count > 0 && newBankVDList.Count > 0)
            {
                // * 企业凭证集合循环
                foreach (var corp in newCorpVDList)
                {
                    // * 筛选银行对账单，第三优先级：勾对金额相等，方向相同
                    var recoamount = corp.Item<decimal>("newrecoamount");
                    var thisrecoamount = corp.Item<decimal>("thisrecoamount");
                    var cd = corp.Item<bool>("cd");
                    var bankVd = newBankVDList.FirstOrDefault(x => x.Item<decimal>("newrecoamount") == recoamount &&
                                                                   x.Item<bool>("cd") == cd);
                    if (bankVd?.Count > 0)
                    {
                        // * 待修改到DB的银行对账单
                        var updateBankVD = new SData();
                        bankVd.ForEachKeys(x => { updateBankVD.Append(x, bankVd[x]); });
                        updateBankVD.Append("bparent", corp["id"]);
                        updateBankVDList.Add(updateBankVD);

                        // * 待修改到DB的企业凭证
                        corp.Append("thisrecoamount", recoamount + thisrecoamount); // 本次勾对金额
                        corp.Append("newrecoamount", 0); // 本次剩余未勾对金额

                        // * 银行对账单集合移除该条银行对账单（剩余银行对账单集合）
                        newBankVDList.Remove(bankVd);
                        bankVDList.Remove(bankVd);
                    }
                }
            }
        }

        /// <summary>
        /// 企业凭证和银行对账单勾对(第四优先级：业务日期升序)
        /// 1、企业凭证集合循环
        /// 2、筛选银行对账单
        /// 3、待修改到DB的银行对账单
        /// 4、待修改到DB的企业凭证
        /// 5、银行对账单集合移除该条银行对账单（剩余银行对账单集合）
        /// 6、企业凭证集合移除该条企业凭证（剩余企业凭证集合）
        /// </summary>
        /// <param name="corpVDList">企业凭证</param>
        /// <param name="bankVDList">银行对账单</param>
        /// <param name="updateBankVDList">返回待修改到DB的银行对账单</param>
        /// <param name="delBankVDList">需要删除的原银行对账单集合</param>
        private void FourReco(List<SData> corpVDList, List<SData> bankVDList, List<SData> updateBankVDList, List<SData> delBankVDList)
        {
            var newCorpVDList = corpVDList.Where(x => x.Item<decimal>("newrecoamount") != 0).OrderBy(x => x.Item<string>("bdate")).ToList();
            if (newCorpVDList.Count > 0 && bankVDList.Count > 0)
            {
                // * 企业凭证集合循环
                foreach (var corp in newCorpVDList)
                {
                    // * 筛选银行对账单，第四优先级：业务日期升序，方向相同
                    var cd = corp.Item<bool>("cd");

                    // newBankVDList有添加需要每次查询时排序
                    var bankVDRList = bankVDList.Where(x => x.Item<bool>("cd") == cd).OrderBy(x => x.Item<string>("bdate")).ToList();

                    if (bankVDRList.Count > 0)
                    {
                        foreach (var bankVd in bankVDRList)
                        {
                            var recoamount = corp.Item<decimal>("newrecoamount");
                            if (recoamount == 0)
                            {
                                break;
                            }
                            var thisrecoamount = corp.Item<decimal>("thisrecoamount");

                            // 对账单勾对金额金额合计
                            var bkRecoamount = bankVd.Item<decimal>("newrecoamount");

                            // * 凭证金额与对账单金额合计比较
                            if (recoamount == bkRecoamount) // 等于
                            {
                                // * 待修改到DB的银行对账单
                                var updateBankVD = new SData();
                                bankVd.ForEachKeys(x => { updateBankVD.Append(x, bankVd[x]); });
                                updateBankVD.Append("bparent", corp["id"]);
                                updateBankVDList.Add(updateBankVD);

                                // * 待修改到DB的企业凭证
                                corp.Append("thisrecoamount", thisrecoamount + recoamount); // 本次勾对金额
                                corp.Append("newrecoamount", 0); // 本次剩余勾对金额

                                // * 银行对账单集合移除该条银行对账单（剩余银行对账单集合）
                                bankVDList.Remove(bankVd);
                            }
                            else if (recoamount > bkRecoamount) // 大于
                            {
                                // * 待修改到DB的银行对账单
                                var updateBankVD = new SData();
                                bankVd.ForEachKeys(x => { updateBankVD.Append(x, bankVd[x]); });
                                updateBankVD.Append("bparent", corp["id"]);
                                updateBankVDList.Add(updateBankVD);

                                // * 银行对账单集合移除该条银行对账单（剩余银行对账单集合）
                                bankVDList.Remove(bankVd);

                                // * 企业凭证集合中修改该条企业凭证勾对金额（剩余企业凭证集合）
                                //  找下一条银行对账单勾对完全,最后找不到则企业凭证勾对不完
                                corp.Append("thisrecoamount", thisrecoamount + bkRecoamount); // 本次勾对金额
                                corp.Append("newrecoamount", (recoamount - bkRecoamount).ToString().ToDec()); // 本次剩余勾对金额
                            }
                            else // 小于
                            {
                                decimal bkamount = recoamount - bkRecoamount; // 银行对账单未勾对金额

                                // * 拆分银行对账单（删除原对账单，添加拆分后的已勾对对账单，）
                                // ---该批中某条银行对账单拆分出未勾对的数据
                                var yes = new SData();
                                var no = new SData();
                                bankVd.ForEachKeys(x =>
                                {
                                    yes.Append(x, bankVd[x]);
                                    no.Append(x, bankVd[x]);
                                });
                                SpiltBankVD(bankVd, bkamount, ref yes, ref no);

                                // ---银行对账单集合添加拆分后的银行对账单
                                // 银行对账单集合添加拆分后未勾对玩的对账单
                                bankVDList.Add(no);

                                // * 待新增到DB的银行对账单
                                yes.Append("bparent", corp["id"]);
                                updateBankVDList.Add(yes);

                                // 原拆单b是待新增到DB的银行对账单，那么需要移除b
                                if (bankVd.Item<int>("isinsert") == 1)
                                {
                                    updateBankVDList.Remove(bankVd);
                                }
                                else
                                {
                                    // 删除原对账单
                                    delBankVDList.Add(bankVd);
                                }

                                // * 银行对账单集合移除该条银行对账单（剩余银行对账单集合）
                                bankVDList.Remove(bankVd);

                                // * 待修改到DB的企业凭证
                                corp.Append("thisrecoamount", thisrecoamount + recoamount); // 本次勾对金额
                                corp.Append("newrecoamount", 0); // 本次剩余勾对金额
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 拆分部分勾对的银行对账单为（待勾对的银行对账单集合和未勾对的银行对账单集合）
        /// </summary>
        /// <param name="bankVDList">银行对账单集合</param>
        /// <param name="bankVDYesList">待勾对的银行对账单集合</param>
        /// <param name="delBankVDList">需要删除的银行对账单集合</param>
        /// <returns>返回需要修改的银行对账单集合</returns>
        private List<SData> SpiltBankVD(List<SData> bankVDList, ref List<SData> bankVDYesList, ref List<SData> delBankVDList)
        {
            // 未勾对的银行对账单集合
            var bankVDNoList = new List<SData>();

            foreach (var bankVD in bankVDList)
            {
                // * 拆分部分勾对的银行对账单为（待勾对的银行对账单集合和未勾对的银行对账单集合）
                if (bankVD.Item<decimal>("amountd") + bankVD.Item<decimal>("amountc") != bankVD.Item<decimal>("newrecoamount"))
                {
                    var yes = new SData();
                    var no = new SData();

                    // 拆分后 业务参考相同
                    bankVD.ForEachKeys(x =>
                    {
                        yes.Append(x, bankVD[x]);
                        no.Append(x, bankVD[x]);
                    });

                    // 对对账单进行拆分
                    SpiltBankVD(bankVD, 0, ref yes, ref no);

                    // 添加拆分后的
                    bankVDYesList.Add(yes);
                    bankVDNoList.Add(no);

                    // 待删除的原对账单
                    delBankVDList.Add(bankVD);
                }
                else
                {
                    bankVDYesList.Add(bankVD);
                }
            }

            return bankVDNoList;
        }

        /// <summary>
        /// 一个对账单拆分成两个对账单
        /// </summary>
        /// <param name="bankVD">拆分前的银行对账单</param>
        /// <param name="bkamount">银行对账单未勾对金额</param>
        /// <param name="yes">拆分后待勾对的银行对账单</param>
        /// <param name="no">拆分后未勾对的银行对账单</param>
        private void SpiltBankVD(SData bankVD, decimal bkamount, ref SData yes, ref SData no)
        {
            var cd = bankVD.Item<bool>("cd");
            var amountd = bankVD.Item<decimal>("amountd");
            var amountc = bankVD.Item<decimal>("amountc");
            var recoamount = bankVD.Item<decimal>("newrecoamount");
            var ddesc = bankVD.Item<string>("ddesc");

            decimal amount = 0;  // 原对账单金额
            decimal splitAmount = 0;  // 对账单拆分后金额

            // 指定未勾对金额
            if (bkamount != 0)
            {
                recoamount = amountd + amountc + bkamount;
            }

            if (cd)
            {
                // 贷
                yes.Append("amountc", recoamount);
                var ac = (amountc - recoamount).ToString().ToDec();
                no.Append("amountc", ac);
                amount = amountc;
                splitAmount = ac;
            }
            else
            {
                // 借
                yes.Append("amountd", recoamount);
                var ad = (amountd - recoamount).ToString().ToDec();
                no.Append("amountd", ad);
                amount = amountd;
                splitAmount = ad;
            }

            var splitA = amount - splitAmount; // 为了补两个00
            var yesddesc = $"对账单{amount}（拆分{splitA}）";
            var noddesc = $"对账单{amount}（拆分{splitAmount}）";
            if (!string.IsNullOrEmpty(ddesc))
            {
                yesddesc += $"，{ddesc}";
                noddesc += $"，{ddesc}";

                // 替换拆分金额
                if (ddesc.Contains("（拆分"))
                {
                    var front = ddesc.Sp_First("（");
                    var behind = ddesc.Sp_ExceptFirst("）");
                    yesddesc = $"{front}（拆分{splitA}）{behind}";
                    noddesc = $"{front}（拆分{splitAmount}）{behind}";
                }
            }

            yes.Append("ddesc", yesddesc);
            no.Append("ddesc", noddesc);

            yes.Append("newrecoamount", recoamount);
            no.Append("newrecoamount", splitAmount);

            // 标识是拆分的对账单，需要insert对账单
            yes.Append("isInsert", 1);
            no.Append("isInsert", 1);
        }

        /// <summary>
        /// 新增拆分后不需要勾对和未勾对成功的银行对账单
        /// </summary>
        /// <param name="insertIdList">新增勾对成功的对账单的Id</param>
        /// <param name="bankVdNoList">拆分后不需要勾对的银行对账单</param>
        /// <param name="bankVdYesList">拆分后未勾对成功的银行对账单</param>
        /// <param name="loginUser">登录人</param>
        private void InsertBankVdNoReco(List<int> insertIdList, List<SData> bankVdNoList, List<SData> bankVdYesList, string loginUser)
        {
            // 过滤拆分后勾对不成功的对账单
            var bankVdYesNoList = bankVdYesList.Where(x => x.Item<int>("isInsert") == 1).ToList();

            // 遍历勾对成功的对账单id
            foreach (var id in insertIdList)
            {
                var newBankVdNo = bankVdNoList.FirstOrDefault(x => x.Item<int>("id") == id);
                var newBankVdYesNo = bankVdYesNoList.FirstOrDefault(x => x.Item<int>("id") == id);
                if (newBankVdNo?.Count > 0 && newBankVdYesNo?.Count > 0)
                {
                    // 将部分勾对拆分后不需要勾对的对账单和拆分后勾对不成功的对账单合并
                    var insertBankVd = new SData();
                    newBankVdYesNo.ForEachKeys(x => { insertBankVd.Append(x, newBankVdYesNo[x]); });

                    // 合并金额
                    var cd = newBankVdNo.Item<bool>("cd");
                    decimal amount = 0;
                    if (cd)
                    {
                        // 贷
                        amount = newBankVdNo.Item<decimal>("amountc") + newBankVdYesNo.Item<decimal>("amountc");
                        insertBankVd.Append("amountc", amount);
                    }
                    else
                    {
                        // 借
                        amount = newBankVdNo.Item<decimal>("amountd") + newBankVdYesNo.Item<decimal>("amountd");
                        insertBankVd.Append("amountd", amount);
                    }

                    // 合并摘要金额
                    var ddesc = newBankVdNo.Item<string>("ddesc");
                    if (!string.IsNullOrEmpty(ddesc) && ddesc.Contains("（拆分"))
                    {
                        var front = ddesc.Sp_First("（");
                        var behind = ddesc.Sp_ExceptFirst("）");
                        ddesc = $"{front}（拆分{amount}）{behind}";
                    }

                    insertBankVd.Append("ddesc", ddesc);

                    AddBankVD(insertBankVd, loginUser);
                }
                else if (newBankVdNo?.Count > 0)
                {
                    // 新增部分勾对拆分后不需要勾对的对账单（因为另一半已经勾对成功）
                    AddBankVD(newBankVdNo, loginUser);
                }
                else if (newBankVdYesNo?.Count > 0)
                {
                    // 新增拆分后勾对不成功的对账单（因为另一半已经勾对成功）
                    AddBankVD(newBankVdYesNo, loginUser);
                }
            }
        }

        /// <summary>
        /// 新增拆分后的对账单集合
        /// </summary>
        /// <param name="insertBankVDList">对账单集合</param>
        /// <param name="loginUser">当前用户</param>
        private void AddBankVDList(List<SData> insertBankVDList, string loginUser)
        {
            foreach (var bank in insertBankVDList)
            {
                AddBankVD(bank, loginUser);
            }
        }

        /// <summary>
        /// 新增拆分后的对账单
        /// </summary>
        /// <param name="bank">对账单</param>
        /// <param name="loginUser">当前用户</param>
        private void AddBankVD(SData bank, string loginUser)
        {
            var bparent = bank.Item<int>("bparent");

            // 摘要超长时截取
            var ddesc = AcVoucherHelper.GetStr(bank.Item<string>("ddesc"), 300);
            var istBank = new SData();
            istBank.Append("ignorecheck", 1);
            istBank.Append("subunit", $"[{bank["subunit.id"]}]");
            istBank.Append("bkaccount", $"[{bank["bkaccount.id"]}]");
            istBank.Append("bank", $"[{bank["bank.id"]}]");
            istBank.Append("bdate", bank["bdate"]);
            istBank.Append("period", bank["period"]);
            istBank.Append("ddesc", ddesc);
            istBank.Append("reference", bank["reference"]);
            istBank.Append("amountd", bank["amountd"]);
            istBank.Append("amountc", bank["amountc"]);

            if (bparent > 0)
            {
                // 添加勾对成功后的值
                istBank.Append("bparent", bparent);
                istBank.Append("rstate", AcConst.AllTick);
                istBank.Append("logicsign", AcConst.RecoManual);
                istBank.Append("recoperson", loginUser);
                istBank.Append("recoamount", bank["newrecoamount"]);
                istBank.Append("recodate", DateTime.Now.ToString("yyyy-MM-dd"));
            }
            else
            {
                istBank.Append("rstate", AcConst.NeverTick);
            }

            ApiTask.Biz<BizTable>("AcBankVD").Insert(istBank);
        }
        #endregion

        #region 撤销勾对

        /// <summary>
        /// 撤销勾对
        /// 1、查询跟银行对账单对账的企业凭证
        /// 2、查询跟企业凭证对账的银行对账单
        /// 3、更新企业凭证和银行对账单
        /// </summary>
        /// <param name="qParm">查询及勾对数据</param>
        private void DeleteReco(SData qParm)
        {
            // * 查询跟银行对账单对账的企业凭证
            var bankVDIds = qParm.Item<string>("bankVDIds"); // 对账单Id
            var updateCpSql = string.Empty; // 修改银行对账单对应的企业凭证
            var updateBkSql = string.Empty; // 修改银行对账单sql
            var trialId = ConstResourceDB.GetId(this.ApiTask, nameof(AcVstate), AcConst.Trial); // 记账状态下子状态可以为正常或锁定
            var rstateId = ConstResourceDB.GetId(this.ApiTask, nameof(AcVSubstate), AcConst.NormalS);
            var rstateLockId = ConstResourceDB.GetId(this.ApiTask, nameof(AcVSubstate), AcConst.LockS);
            if (!string.IsNullOrEmpty(bankVDIds))
            {
                var bankVDIdsql = string.Empty; // 对账id组合
                var bankVDIdList = bankVDIds.ToList();
                foreach (var id in bankVDIdList)
                {
                    bankVDIdsql += $"id={id} or ";
                }

                // 找出银行对账单的a4(即企业凭证id)和d0
                var bparentsql = $@"select a4 from voucherd where ({bankVDIdsql} 1=0 )";

                // 解决1个凭证对应多个对账单，撤销对账单，凭证未撤销，凭证勾对金额-对账单d0之和
                var bparentgroupsql = $@"select sum(d0) as d0,a4 from voucherd where ({bankVDIdsql} 1=0 ) group by a4";

                // 银行对账单对应的企业凭证----更新凭证分录：勾对金额、勾对状态、父id
                updateCpSql = $@"
update m 
set a9=(select id from others where dclass='acrecostate' and dcode=(case when (isnull(m.d0,0)-d.d0)=0 then '{AcConst.NeverTick}' else '{AcConst.PartTick}' end)),
d0=(isnull(m.d0,0)-d.d0),
a8=(case when (isnull(m.d0,0)-d.d0)=0 then 0 else m.a8 end),
a7=(case when (isnull(m.d0,0)-d.d0)=0 then 0 else m.a7 end),
s0=(case when (isnull(m.d0,0)-d.d0)=0 then '' else m.s0 end),
a4=(case when (isnull(m.d0,0)-d.d0)=0 then null else m.a4 end),
vsubstate=(case when m.vstate={trialId} and (isnull(m.d0,0)-d.d0)=0 then {rstateId} else m.vsubstate end) 
from 
voucherd as m 
 join ({bparentgroupsql}) as d on m.id=d.a4; 
update m 
set vsubstate=(case when m.vstate={trialId} then {rstateId} else m.vsubstate end) 
from 
voucher as m 
where 
exists(select vh from voucherd d where exists ({bparentsql} and a4=d.id) and d.vh=m.id) And 
not exists(select vh from voucherd where vh=m.id and vsubstate={rstateLockId});";

                // 更新银行对账单：清空：勾对状态、勾对金额、勾对类型、勾对人、勾对日期、父id
                updateBkSql = $@" 
update m 
set a9=(select id from others where dclass='acrecostate' and dcode='{AcConst.NeverTick}'),
d0=0,
a8=0,
a7=0,
s0='',
a4=null 
from 
voucherd as m 
where 
({bankVDIdsql} 1=0);";
            }

            // * 查询跟企业凭证对账的银行对账单
            var corpVDIds = qParm.Item<string>("corpVDIds"); // 企业凭证id
            var updateCpVdSql = string.Empty; // 修改勾对的企业凭证
            var updateBkVdSql = string.Empty; // 修改凭证对应的银行对账单

            if (!string.IsNullOrEmpty(corpVDIds))
            {
                var bparentsql = string.Empty; // 凭证id组合的parent
                var corpVDIdsql = string.Empty; // 凭证idsql
                var notCorpIdsql = string.Empty; // 非凭证idsql
                var bankIdsql = string.Empty; // 企业凭证对应的对账单sql
                var corpVDIdList = corpVDIds.ToList();
                foreach (var id in corpVDIdList)
                {
                    bparentsql += $"a4={id} or ";
                    corpVDIdsql += $"id={id} or ";
                    notCorpIdsql += $"id!={id} and ";
                }

                // 找出企业凭证（parent是企业凭证id）的银行对账单
                bankIdsql = $@"select id from voucherd where ({bparentsql} 1=0 ) and ({notCorpIdsql}1=1)";

                // 勾选的企业凭证----更新凭证分录：勾对金额、勾对状态、父id
                updateCpVdSql = $@" 
update m 
set a9=(select id from others where dclass='acrecostate' and dcode='{AcConst.NeverTick}'),
d0=0,
a8=0,
a7=0,
s0='',
a4=null,
vsubstate=(case when m.vstate={trialId} then {rstateId} else m.vsubstate end) 
from 
voucherd as m 
where {corpVDIdsql}1=0; 
update m 
set vsubstate=(case when m.vstate={trialId} then {rstateId} else m.vsubstate end) 
from 
voucher as m 
where 
exists(select vh from voucherd where ({corpVDIdsql}1=0) and vh=m.id) And 
not exists(select vh from voucherd where vh=m.id and vsubstate={rstateLockId});";

                // 更新银行对账单：清空：勾对状态、勾对金额、勾对类型、勾对人、勾对日期、父id
                updateBkVdSql = $@" 
update m 
set a9=(select id from others where dclass='acrecostate' and dcode='{AcConst.NeverTick}'),
d0=0,
a8=0,
a7=0,
s0='',
a4=null 
from 
voucherd as m 
where 
exists ({bankIdsql} and m.id = id);";
            }

            // * 更新企业凭证和银行对账单
            var sql = updateCpSql + updateBkSql + updateCpVdSql + updateBkVdSql;
            if (!string.IsNullOrEmpty(sql))
            {
                this.ApiTask.DB.ExecuteNonQuery(sql);
            }
        }

        #endregion

        #endregion
    }
}
