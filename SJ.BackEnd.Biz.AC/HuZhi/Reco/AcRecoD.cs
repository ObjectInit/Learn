#region << 版 本 注 释 >>
/* ==============================================================================
// <copyright file="ACRecoD.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：勾对体
* 创 建 人：胡智
* 创建日期：2019-09-26 15:51:22
* ==============================================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 勾对体
    /// 1.描述：撤销勾对、根据凭证Id获取同批次的所有勾对记录业务处理类
    /// 2.约定
    ///     1.批量新增勾对记录,约定recod存除第一条记录的其他勾对记录：
    ///       WebApiClient.Insert("AcRecoD", new SData("recoamount", 100, "t29", 502, "cd", 0, "logicsign", "z1", "parent", 1,
    ///                          "recod", new List<SData>(){new SData("recoamount", 100, "t29", 502, "cd", 1, "logicsign", "z1", "parent", 2)}))
    ///     2.前端撤销勾对：WebApiClient.Delete("AcRecoD", new SData("id", -1, "batch", batchlist, "rver", -1))
    ///     2.GetList方法中约定一个键type：recoquery(根据凭证分录id,获取同批次勾对记录); 约定一个键vIds：凭证分录id(多个用,号分割)
    ///     3.约定一个固定勾对科目Z90000、parent存凭证分录Id
    /// 3.业务逻辑
    ///     1.新增前：插入勾对头，手工勾对(第一条记录需要验证勾对逻辑),自动勾对、单据勾对无需校验逻辑
    ///     2.删除前
    ///         1.单据勾对 (逻辑判断，不删除勾对记录)
    ///         2.修改勾对状态
    ///         3.（手动或自动）根据批次删除勾对记录
    ///     4.查询前：
    ///         1.根据凭证分录id获取对应勾对记录的批次
    ///         2.登录人核算单位不为空，添加核算单位查询参数
    ///         3.设置默认按批次,id排序
    /// </summary>
    [ClassData("cname", "勾对体", "vision", 1)]
    public class AcRecoD : BizTableVoucherD
    {
        /// <summary>
        /// 定义预留字段
        /// </summary>
        protected override void OnCustomDefine()
        {
            // 字段定义 **********
            // 勾对状态
            this.AddField("rstate", ApiTask.L("勾对状态"), EFieldType.关联, "AcRecoState", "a9");

            // 收付款单
            this.AddField("recpay", ApiTask.L("收付款单头"), EFieldType.整数, string.Empty, "a6");

            // 勾对汇总单据 todo
            // this.AddField("recosum", "勾对汇总单据", EFieldType.关联, string.Empty, "a5");

            // 下设科目对账属性
            AcVoucherHelper.AddRecoField(this);

            // 逻辑标志
            this.AddField("logicsign", ApiTask.L("勾对类型"), EFieldType.关联, "ACRecoType", "a8");

            // 勾对人
            this.AddField("recoperson", ApiTask.L("勾对人"), EFieldType.关联, "AcUser", "a7");

            // 勾对金额
            this.AddField("recoamount", ApiTask.L("勾对金额"), EFieldType.数值, string.Empty, "d0");

            // 批次
            this.AddField("batch", ApiTask.L("批次"), EFieldType.字符串, string.Empty, "s0");

            // 摘要
            ((FieldDefine)ListField["ddesc"]).DisplayName = ApiTask.L("摘要");

            // 勾对日期
            ((FieldDefine)ListField["bdate"]).DisplayName = ApiTask.L("勾对日期");

            // 借贷方向
            ((FieldDefine)ListField["cd"]).DisplayName = ApiTask.L("借贷方向");

            // 关联勾对头
            ((FieldDefine)ListField["vh"]).RefBiz = "AcReco";

            // 查询凭证分录
            this.AddSubQuery("acvoucherd", "凭证分录", "AcVoucherD", "id=parent");
            this.AddSubQuery("acvoucherdsub", "凭证分录sub", "AcVoucherDSub", "id=parent");

            base.OnCustomDefine();
        }

        #region 框架方法重写

        /// <summary>
        /// 静态过滤条件
        /// 1.设置科目为勾对科目
        /// </summary>
        public override SData BaseParms => new SData("account", $"[{ConstResourceDB.GetAccount(this.ApiTask, AcConst.RecoAccount)}]");//ConstResource.RecoAccount);

        /// <summary>
        /// 新增前逻辑
        /// 1.插入勾对头
        /// 2.手工勾对(第一条记录需要验证勾对逻辑),自动勾对、单据勾对无需校验逻辑
        /// 3.初始化勾对体数据
        /// </summary>
        /// <param name="data">勾对体数据</param>
        protected override void OnInsertBefore(SData data)
        {
            if (data == null || data.Count == 0)
            {
                throw new Exception(ApiTask.LEnd("无勾对的数据"));
            }

            // 勾对类型 (手工勾对、自动勾对、单据勾对)
            string type = data.Item<string>("logicsign");
            if (string.IsNullOrEmpty(type))
            {
                throw new Exception(ApiTask.LEnd("无勾对的数据"));
            }

            // 勾对类型存在性验证
            this.CheckRefExist(data, "logicsign");

            // 数据准备 **********
            // 变量声明
            // 是否第一条插入的勾对记录(空的时候为第一条勾对记录)
            string key = data.Item<string>("isfirst");

            data["recoId"] = AcVoucherHelper.GetRecoId(this.ApiTask);

            // * 手工勾对(第一条记录需要验证勾对逻辑),自动勾对、单据勾对无需校验逻辑
            if (type.Sp_First(".").Equals(AcConst.RecoManual))
            {
                // 第一条勾对记录，验证勾对逻辑
                if (string.IsNullOrEmpty(key))
                {
                    var lstRecod = GetAllRecoD(data);
                    if (lstRecod.Count == 0)
                    {
                        throw new Exception(ApiTask.LEnd("无勾对的数据"));
                    }

                    // * 勾对体金额校验
                    foreach (var recoD in lstRecod)
                    {
                        // 勾对金额校验（不能为空，不能为0，是数值）
                        AcVoucherHelper.CheckDecimal(this.ApiTask, recoD.Item<string>("recoamount"), ((FieldDefine)ListField["recoamount"]).DisplayName);
                    }

                    // * 勾对体逻辑验证
                    AddRecoDBusinessLogic(lstRecod);
                }
            }

            // * 初始化勾对体数据
            InitData(data);

            base.OnInsertBefore(data);
        }

        /// <summary>
        /// 新增后逻辑
        /// 1.isfirst空的时候为第一条勾对记录,插入其他勾对记录
        /// 2.手动勾对后更新勾对状态
        /// </summary>
        /// <param name="data">勾对体数据</param>
        protected override void OnInsertAfter(SData data)
        {
            // 数据准备 **********
            // 变量声明
            var logicsign = data.Item<string>("logicsign").Sp_First(".");

            // 是否第一条插入的勾对记录(空的时候为第一条勾对记录)
            string key = data.Item<string>("isfirst");
            if (string.IsNullOrEmpty(key))
            {
                var recodList = data.Item<List<SData>>("recod");
                if (recodList != null && recodList.Count > 0)
                {
                    // * 插入其他勾对记录
                    foreach (SData recod in recodList)
                    {
                        recod.Append("isfirst", "false");
                        recod.Append("batch", data.Item<string>("batch"));
                        this.Insert(recod);
                    }

                    // * 手动勾对后更新勾对状态
                    if (logicsign.Equals(AcConst.RecoManual))
                    {
                        var subrecId = int.Parse(data[$"{AcConst.RecoTcode}.id"].ToString());
                        AcVoucherHelper.UpdateRecoState(this, data["batch"].ToString(), subrecId);
                    }
                }
            }

            if (!logicsign.Equals(AcConst.RecoVoucher))
            {
                // * 勾对或收付款时-修改凭证头和凭证体子状态为锁定
                var idList = new List<int>();
                idList.Add(data.Item<int>("parent"));
                AcVoucherHelper.UpdateVSubstateLock(this, idList);
            }

            base.OnInsertAfter(data);
        }

        /// <summary>
        /// 修改逻辑前
        /// </summary>
        /// <param name="oldData">修改前勾对记录数据</param>
        /// <param name="updateData">修改后勾对记录数据</param>
        protected override void OnUpdateBefore(SData oldData, SData updateData)
        {
            // 将勾对头的id赋给勾对体的vh
            updateData["vh"] = $"[{oldData["vh.id"]}]";

            // 给借方金额赋值
            var cd = updateData["cd", oldData.Item<string>("cd")].ToString();
            if (!string.IsNullOrEmpty(cd) && cd.Equals("0"))
            {
                updateData["amountd"] = updateData["amountd", updateData["recoamount"]];
                updateData["amountc"] = updateData["amountc", 0];
            }
            else
            {
                updateData["amountc"] = updateData["amountc", updateData["recoamount"]];
                updateData["amountd"] = updateData["amountd", 0];
            }

            // 给amount赋值
            updateData["amount"] = updateData["amount", updateData["amountd"].ToString().ToDec() - updateData["amountc"].ToString().ToDec()];

            base.OnUpdateBefore(oldData, updateData);
        }

        protected override void OnUpdateAfter(SData OldData, SData UpdateData, SData Modify)
        {
            base.OnUpdateAfter(OldData, UpdateData, Modify);
        }

        /// <summary>
        /// 获取数据前逻辑
        /// 1.根据凭证分录id获取对应勾对记录的批次(约定传入 type=recoquery,vIds=凭证分录Id)
        /// 2.登录用户核算单位不为空，添加核算单位查询参数(核算单位=登录用户的核算单位)
        /// 3.设置处理日期条件、核算期的区间
        /// 4.设置默认按批次排序
        /// </summary>
        /// <param name="qParm">查询参数</param>
        protected override void OnGetListBefore(SData qParm)
        {
            // 业务逻辑 **********
            // * 根据凭证分录id获取对应勾对记录的批次(约定传入 type=recoquery,vIds=凭证分录Id)
            if (!string.IsNullOrEmpty(qParm.Item<string>("type")) && qParm.Item<string>("type").Equals(AcConst.RecoQuery))
            {
                // 获取勾对记录的批次,多个用,号隔开
                string batchs = GetRecoBatch(qParm);

                // 批次不为空添加到查询参数中
                if (!string.IsNullOrEmpty(batchs))
                {
                    qParm.Append("batch", batchs);
                }
                else
                {
                    qParm.Append("batch", "-1");
                }
            }

            // 传入核算单位为空
            if (string.IsNullOrEmpty(qParm.Item<string>("subunit")))
            {
                // * 登录用户核算单位不为空，添加核算单位查询参数(核算单位=登录用户的核算单位)
                if (!string.IsNullOrEmpty(ApiTask.UserInfo().UserSubUnit()))
                {
                    qParm.Append("subunit", ApiTask.UserInfo().UserSubUnit());
                }
            }
            else
            {
                // 传入核算单位不为空(核算单位=传入核算单位)
                qParm.Append("subunit", qParm.Item<string>("subunit"));
            }

            // * 处理日期条件、核算期的区间
            qParm.ConvertDate("bdate");
            qParm.ConvertDate("period");

            // * 设置默认按批次排序
            qParm.Append("q.orderby", "batch,id");

            base.OnGetListBefore(qParm);
        }

        /// <summary>
        /// 删除前逻辑
        /// 1.撤销勾对id=-1
        /// 2.直接sql删除勾对体不删除勾对体
        /// </summary>
        /// <param name="data">需要删除的勾对头数据</param>
        protected override void OnDeleteBefore(SData data)
        {
            // new SData("id", -1, "batch", batchlist)
            // batchlist：new SData("batch", recoD["batch"], "logicsign", "z1", "t29", recoD["t29.id"])
            if (data["id"].Equals(-1))
            {
                // 数据准备 **********
                // 批次
                var batchList = data.Item<List<SData>>("batch");

                // 业务逻辑 **********
                foreach (var batchItem in batchList)
                {
                    var type = batchItem["logicsign"].ToString();
                    var batch = batchItem["batch"].ToString();

                    // * 勾对记录是单据勾对 (判断是付款单生成的还是收款单生成的)
                    if (AcConst.RecoVoucher.Equals(type))
                    {
                        throw new Exception(ApiTask.LEnd(CheckRecoIsCancel(batch)));
                    }
                }

                var recoAccount = ConstResourceDB.GetAccount(this.ApiTask, AcConst.RecoAccount);

                // * 勾对或收付款时-修改凭证头和凭证体子状态为正常
                var batchStr = string.Join(",", batchList.Select(x => x.Item<string>("batch")));
                if (!string.IsNullOrEmpty(batchStr))
                {
                    var recoDList = ApiTask.Biz<BizTable>("AcRecoD").GetListData(0, 0, new SData("batch", batchStr, "account", recoAccount).toParmStr(), "id,parent");
                    var idList = recoDList.Select(x => x.Item<int>("id")).ToList(); // 勾对记录id
                    var parentList = recoDList.Select(x => x.Item<int>("parent")).Distinct().ToList(); // 凭证id
                    AcVoucherHelper.UpdateVSubstateNoLock(this, parentList, idList);
                }

                foreach (var batchItem in batchList)
                {
                    var type = batchItem["logicsign"].ToString();
                    var batch = batchItem["batch"].ToString();

                    // * 非单据勾对，更新勾对状态
                    if (!AcConst.RecoVoucher.Equals(type))
                    {
                        var subrecId = int.Parse(batchItem[AcConst.RecoTcode].ToString());
                        AcVoucherHelper.UpdateRecoStateByDelete(this, batch, subrecId);

                        // * 根据批次删除勾对记录
                        this.ApiTask.DB.ExecuteNonQuery($"delete from voucherd where s0='{batch}' and account={recoAccount}");
                    }
                }
            }

            base.OnDeleteBefore(data);
        }

        ///// <summary>
        ///// 更新勾对状态
        ///// </summary>
        ///// <param name="data">勾对体数据</param>
        //protected override void OnDeleteAfter(SData data)
        //{

        //}

        #endregion

        #region 自定义方法

        /// <summary>
        /// 手动勾对逻辑校验
        /// </summary>
        /// <param name="data">勾对体对象</param>
        private void AddRecoDBusinessLogic(List<SData> lstRecod)
        {
            // 数据准备 **********
            // 变量声明
            string subjectRecId = lstRecod[0][AcConst.RecoTcode].ToString(); // 科目对账Code
            var voucherTable = ApiTask.Biz<BizTable>("AcVoucherD"); // 凭证分录业务对象
            var recodTable = ApiTask.Biz<BizTable>("AcRecoD"); // 勾对体业务对象
            List<SData> lstVoucher = new List<SData>(); // 存勾对体对应的凭证体
            decimal debitTickPairSum = 0;  // 借方勾对金额和
            decimal creditTickPairSum = 0; // 贷方勾对金额和  

            // 获取科目对账和科目之间关系的列表
            List<SData> subjectRelationList = ApiTask.Biz<BizTable>("AcReconRef").GetListData(0, 0, new SData("subrec", lstRecod[0][AcConst.RecoTcode]).toParmStr(), "id,account,subrec,isreverse");

            // 获取科目对账中勾选的Tcode
            List<string> tcodeList = GetTcodes(subjectRecId);
            string columns = "id,vstate,cd,amountd,amountc,subunit,account,vclass.dcode"; // Tcode列名

            // 勾对记录对应的所有凭证Id
            var parentIdList = lstRecod.Select(x => x["parent"].ToString()).ToList();

            string tcodes = string.Join(",", tcodeList);
            if (!string.IsNullOrEmpty(tcodes))
            {
                columns += "," + tcodes;
            }

            // 获取凭证的已勾对金额（勾对体状态z5）+待审金额（勾对体状态<z5）
            var queryFields = new List<string>() { "parent", "a_sum_recoamount" };
            var groupFields = new List<string>() { "parent" };

            // 分300/页 取凭证id
            var parentIds = AcVoucherHelper.GetIdsByPage(parentIdList);
            var recoamountList = new List<SData>();
            var voucherList = new List<SData>();
            foreach (var parentId in parentIds.Keys)
            {
                var pIds = string.Join(",", parentIds.Item<List<string>>(parentId).ToList());

                // 获取勾对记录对应的所有凭证
                var vdList = voucherTable.GetListData(0, 1, new SData("id", pIds).toParmStr(), columns);
                voucherList.AddRange(vdList);

                // 获取凭证的已勾对金额
                var sql = recodTable.QuerySql(new SData(AcConst.RecoTcode, subjectRecId, "parent", pIds), queryFields, groupFields, null, false);
                var list = recodTable.ApiTask.DB.ExecuteList(sql).ToList();
                recoamountList.AddRange(list);
            }

            // 业务逻辑 **********
            // 遍历需要添加的勾对体
            foreach (var recoD in lstRecod)
            {
                int parentId = int.Parse(recoD["parent"].ToString()); // 勾对体的父Id(凭证的Id)
                var voucher = voucherList.FirstOrDefault(x => x["id"].ToString().ToInt() == parentId); // 根据勾对体父Id 获取凭证分录信息

                // * 凭证分录是否存在
                if (voucher == null)
                {
                    throw new Exception(ApiTask.LEnd("所选记录不存在"));
                }

                lstVoucher.Add(voucher);

                // * 凭证分录不是实际数据，提示：所选记录不是实际数据，不允许勾对
                if (!voucher["vclass.dcode"].ToString().StartsWith(AcConst.ActData))
                {
                    throw new Exception(ApiTask.LEnd("所选记录不是实际数据，不允许勾对"));
                }

                // * 凭证分录是否未记账
                if (!voucher["vstate"].ToString().StartsWith(AcConst.Trial))
                {
                    throw new Exception(ApiTask.LEnd("所选记录未记账，不允许勾对"));
                }

                // * 凭证分录的科目是否等于科目对账定义中的对账科目
                var subjectRelation = subjectRelationList.FirstOrDefault(s => s["account"].ToString() == voucher["account"].ToString());
                if (subjectRelation == null)
                {
                    throw new Exception(ApiTask.LEnd("所选记录科目不属于对账科目，不允许勾对"));
                }

                // 聚合查询得到已勾对金额
                object recoAmountObject = recoamountList.FirstOrDefault(x => x["parent"].ToString().ToInt() == parentId)?.Item<decimal>("a_sum_recoamount");
                decimal recoAmount = recoAmountObject?.ToString().ToDec() ?? 0;

                // 凭证分录未勾对金额 (未勾对金额：借方金额 + 贷方金额 - 待审金额 - 已勾金额)
                decimal noRecoAmount = voucher["amountd"].ToString().ToDec() + voucher["amountc"].ToString().ToDec() - recoAmount;

                // 未勾对金额为负数
                if (noRecoAmount < 0)
                {
                    // * 凭证分录的勾对的金额是否大于未勾对的金额
                    if (Math.Abs(recoD["recoamount"].ToString().ToDec()) > Math.Abs(noRecoAmount))
                    {
                        throw new Exception(ApiTask.LEnd("所选记录勾对金额大于未勾对金额，不允许勾对"));
                    }
                }
                else
                {
                    // * 凭证分录的勾对的金额是否大于未勾对的金额
                    if (recoD["recoamount"].ToString().ToDec() > noRecoAmount)
                    {
                        throw new Exception(ApiTask.LEnd("所选记录勾对金额大于未勾对金额，不允许勾对"));
                    }
                }

                #region 统计借方勾对金额和、和贷方勾对金额和

                // 科目对账定义中设置了反向,金额 * -1
                if (subjectRelation["IsReverse"].ToString().ToLower().Equals("true"))
                {
                    // 勾对金额在借方
                    if (recoD["cd"].ToString().ToInt() == AcConst.Debit)
                    {
                        debitTickPairSum = debitTickPairSum + (recoD["recoamount"].ToString().ToDec() * -1);
                    }
                    else
                    {
                        creditTickPairSum = creditTickPairSum + (recoD["recoamount"].ToString().ToDec() * -1);
                    }
                }
                else
                {
                    if (recoD["cd"].ToString().ToInt() == AcConst.Debit)
                    {
                        debitTickPairSum += recoD["recoamount"].ToString().ToDec();
                    }
                    else
                    {
                        creditTickPairSum += recoD["recoamount"].ToString().ToDec();
                    }
                }

                #endregion

            }

            // * 校验勾对凭证分录的分析（Tcode）项
            if (!CheckTCodeIsSame(lstVoucher, tcodeList))
            {
                throw new Exception(ApiTask.LEnd("所选记录不符合对账规则，不允许勾对"));
            }

            // * 检查凭证体的核算单位是否相同
            if (!CheckSubUnitIsSame(lstVoucher))
            {
                throw new Exception(ApiTask.LEnd("所选记录核算单位不同，不允许勾对"));
            }

            // * 凭证分录的借方勾对金额合计是否等于贷方勾对金额合计
            if (debitTickPairSum != creditTickPairSum)
            {
                throw new Exception(ApiTask.LEnd("所选记录借贷不平衡，不允许勾对"));
            }
        }

        /// <summary>
        /// 根据科目对账获取科目对账设置的Tcode
        /// 1.获取科目对账
        /// 2.找到启用的Tcode
        /// </summary>
        /// <param name="subrecId">科目对账Id</param>
        /// <returns>返回科目对账设置的Tcode</returns>
        private List<string> GetTcodes(string subrecId)
        {
            // 数据准备 **********
            // 变量声明
            List<string> tCodeList = new List<string>(); // 保存科目对账设置的Tcode

            // * 获取科目对账
            var id = int.Parse(subrecId.Replace("[", "").Replace("]", ""));
            var acSubRec = ApiTask.Biz<BizTable>("AcSubRec").GetItem(id);

            // 业务逻辑 **********
            // * 找到启用的Tcode
            var tcodeValueList = acSubRec.Where(x => Regex.IsMatch(x.Key, Regx.TcodeModel) && x.Value.Equals(true));

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
        /// 所有勾对体
        /// </summary>
        /// <param name="data">勾对体</param>
        /// <returns>所有勾对体集合</returns>
        private List<SData> GetAllRecoD(SData data)
        {
            // 获取勾对体
            var lstRecod = new List<SData>();
            lstRecod.Add(new SData(AcConst.RecoTcode, data[AcConst.RecoTcode], "parent", data["parent"], "recoamount", data["recoamount"], "cd", data["cd"], "logicsign", data["logicsign"]));
            var recodList = data.Item<List<SData>>("recod");
            if (recodList != null && recodList.Count > 0)
            {
                lstRecod.AddRange(data.Item<List<SData>>("recod"));
            }

            return lstRecod;
        }

        /// <summary>
        /// 检查凭证分录Tcode是否相同(科目对账中勾选的Tcode)
        /// 1.记录第一条凭证分录的Tcode
        /// 2.筛选Tcode值相同且不为空的记录
        /// 3.判断传入的凭证分录数，和Tcode值相同的凭证分录数是否相同
        /// </summary>
        /// <param name="lstVoucher">凭证分录</param>
        /// <param name="tcodeValueList">科目对账中勾选的Tcode</param>
        /// <returns>分析项相同返回true,否则返回false</returns>
        private bool CheckTCodeIsSame(List<SData> lstVoucher, List<string> tcodeValueList)
        {
            // 数据准备 **********
            // 变量声明
            Dictionary<string, string> firstTcode = new Dictionary<string, string>(); // 保存第一条凭证分录的Tcode(科目对账中勾选的Tcode)
            int voucherCount = lstVoucher.Count; // 传入的凭证分录数

            List<SData> list = lstVoucher;

            // 业务逻辑 **********
            if (tcodeValueList.Count > 0)
            {
                // * 记录第一条凭证分录的Tcode
                foreach (var tcode in tcodeValueList)
                {
                    firstTcode.Add(tcode, lstVoucher[0][tcode]?.ToString());
                }

                foreach (var tcode in firstTcode)
                {
                    // * 筛选Tcode值相同且不为空的记录
                    list = list.Where(s => s[tcode.Key]?.ToString() == tcode.Value && !string.IsNullOrEmpty(s[tcode.Key]?.ToString())).ToList();
                }

                // * 判断传入的凭证分录数，和Tcode值相同的凭证分录数是否相同
                if (voucherCount != list.Count)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查凭证体的核算单位是否相同
        /// 1.记录原始凭证体的数量
        /// 2.以原始凭证体第一条数据的核算期为条件,筛选出凭证体数量
        /// 3.原始凭证体的数量和筛选后的凭证数量相同返回true,否则返回false
        /// </summary>
        /// <param name="voucherList">凭证体数据</param>
        /// <returns>凭证体的核算单位相同返回true,否则返回false</returns>
        private bool CheckSubUnitIsSame(List<SData> voucherList)
        {
            // 业务逻辑 **********
            // * 记录原始凭证体的数量
            int voucherCount = voucherList.Count;

            // * 以原始凭证体第一条数据的核算期为条件,筛选出凭证体数量
            int subunitCount = voucherList.Where(v => v["subunit"]?.ToString() == voucherList[0]["subunit"]?.ToString()).Select(s => s["id"]).Count();

            // * 原始凭证体的数量和筛选后的凭证数量相同返回true,否则返回false
            return voucherCount == subunitCount ? true : false;
        }

        /// <summary>
        /// 检查勾对体是否可以撤销
        /// 1.获取勾对体
        /// 2.获取勾对体对应的凭证分录(根据凭证分录Id)
        /// 3.凭证分录父id有值,不能勾对(父id有值判断是付款单还是收款单)
        /// </summary>
        /// <param name="id">勾对体id</param>
        /// <param name="message">不能撤销时返回的提示信息,receipt代表收款单,pay代表付款单</param>
        /// <returns>可以勾对返回true,否则返回false</returns>
        private string CheckRecoIsCancel(string batch)
        {
            // 数据准备 **********
            // 获取同一批次的勾对记录
            var recoDList = ApiTask.Biz<BizTable>("AcRecoD").GetListData(0, 0, new SData("batch", batch).toParmStr(), "parent").Select(s => s["parent"]);

            // 获取同一批次的凭证Id
            var voucherIds = string.Join(",", recoDList);

            // 获取一个父id不为0的凭证
            SData voucherd = ApiTask.Biz<BizTable>("AcVoucherD").GetItemByParms(new SData("id", voucherIds, "parent", "!:0").toParmStr(), "parent,account");

            // 获取凭证分录的父Id
            int parentId = voucherd["parent"].ToString().ToInt();

            // 获取收款单体数量
            int receiptdCount = ApiTask.Biz<BizTable>("AcReceiptD").GetCount(new SData("id", parentId, "account.dcode", AcConst.ReceiptAccount).toParmStr());

            // 找不到收款单
            if (receiptdCount <= 0)
            {
                return "通过付款单产生的勾对记录不能撤销";
            }
            else
            {
                return "通过收款单产生的勾对记录不能撤销";
            }
        }

        /// <summary>
        /// 根据凭证分录id获取勾对体的批次(多个用,号隔开)
        /// </summary>
        /// <param name="qParm">传入的凭证分录id(多个用,号隔开)</param>
        /// <returns>返回勾对体的批次</returns>
        private string GetRecoBatch(SData qParm)
        {
            if (!string.IsNullOrEmpty(qParm.Item<string>("vIds")))
            {
                // 根据凭证分录id获取勾对体的批次(多个用,号隔开)
                var recoBatch = ApiTask.Biz<BizTable>("AcRecoD").GetListData(0, 0, new SData("parent", qParm.Item<string>("vIds")).toParmStr(), "batch").Select(s => s["batch"]);
                return string.Join(",", recoBatch);
            }

            return string.Empty;
        }

        /// <summary>
        /// 初始化勾对体数据
        /// 金额，借方金额、贷方金额、科目、勾对人、业务日期、核算单位
        /// </summary>
        /// <param name="data">勾对体数据</param>
        private void InitData(SData data)
        {
            // 给勾对体赋初始值 vh:勾对头id,period: 核算期,batch: 批次,SubUnit: 核算单位,vclass: 数据类型,vstate: 单据状态

            // 给借方金额赋值
            var cd = data.Item<string>("cd");
            if (!string.IsNullOrEmpty(cd) && cd.Equals("0"))
            {
                data.Append("amountd", data["recoamount"]);
                data.Append("amountc", 0);
            }
            else
            {
                data.Append("amountc", data["recoamount"]);
                data.Append("amountd", 0);
            }

            // 给金额赋值
            data.Append("amount", data["amountd"].ToString().ToDec() - data["amountc"].ToString().ToDec());
            data.Append("account", AcConst.RecoAccount);
            data.Append("bdate", DateTime.Now.ToString("yyyy-MM-dd"));

            var subunit = ApiTask.UserInfo().UserSubUnit();
            if (string.IsNullOrEmpty(subunit))
            {
                data.Append("subunit", "z0");
            }
            else
            {
                data.Append("subunit", subunit);
            }

            data.Append("vclass", AcConst.ActData);

            // 单据状态为空，默认已审状态
            if (string.IsNullOrEmpty(data.Item<string>("vstate")))
            {
                data.Append("vstate", AcConst.Trial);
            }

            data.Append("vh", $"[{data["recoId"]}]");
            data.Append("period", GetPeriod());

            // 批次为空创建一个批次
            if (string.IsNullOrEmpty(data.Item<string>("batch")))
            {
                data.Append("batch", AcVoucherHelper.GetRecoBatch(this.ApiTask));
            }

            data.Append("recoperson", ApiTask.UserInfo().UserCode());
        }

        /// <summary>
        /// 获取当前核算期
        /// </summary>
        /// <returns>返回当前核算期</returns>
        private int GetPeriod()
        {
            return PeriodHelper.GetCurrent(this.ApiTask);
        }
        #endregion

    }
}
