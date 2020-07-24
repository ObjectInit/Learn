#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcBVoucher.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcPayReceipt
* 创 建 者：李琳
* 创建时间：2019/12/20 14:17:52
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 收付款基类
    /// 1 描述：收付款审批流程业务处理类
    /// 2 约定：无
    /// 3 业务逻辑
    ///     审批前：
    ///         提交逻辑：检查凭证科目是否在对账科目中、核算单位是否一致，分析项是否一致，是否借贷平衡，修改单据状态到在审状态
    ///         记账逻辑：检查凭证科目是否在对账科目中、核算单位是否一致，分析项是否一致，是否借贷平衡，收款金额是否合法，修改单据状态到已审状态
    ///         反记账逻辑：核算期是否小于当前核算器，修改单据状态到草稿状态
    ///     审批后：
    ///         提交逻辑：清空记账人和记账时间，更新勾对记录
    ///         撤回逻辑：清空记账人和记账时间，更新勾对记录
    ///         记账逻辑：修改业务日期，生成勾对记录，生成新凭证
    ///         反记账逻辑：清空记账人和记账时间，更新勾对记录，删除记账生成的凭证的勾对记录，删除记账生成的新凭证
    /// </summary>
    public class AcPayReceipt : BizTableVoucher
    {
        #region 属性

        /// <summary>
        /// 单据
        /// </summary>
        protected virtual string BizD { get; set; }

        /// <summary>
        /// 单据中文名称
        /// </summary>
        protected virtual string BizName { get; set; }

        #endregion

        #region 框架方法重写

        private int accountId = 0; // 科目
        private int propertyId = 0; // 科目性质

        /// <summary>
        /// 单据审批前,事物处理中,可直接报错回滚
        /// 1、提交和记账需要检查数据
        /// 2、反记账检查核算期是否小当前核算期
        /// </summary>
        /// <param name="Act">
        ///     审批动作flow.id, 流程id
        ///     state1.id, 开始状态id
        ///     state2.id, 开始状态id
        ///     pass.id, 通行证id
        ///     flow, 审批流程（z03.应收应付）
        ///     state1,开始状态（ z1.草稿）
        ///     substate1, 开始子状态
        ///     state2, 结束状态（z3.待审）
        ///     substate2, 结束子状态
        ///     pass,通行证（ z0110.凭证录入）
        /// </param>
        /// <param name="Data">当前单据头数据</param>
        protected override void OnApproveBefore(SData Act, SData Data)
        {
            // 判断操作为提交或记账
            if ((Act["state1"].ToString().StartsWith(AcConst.Draft) && Act["state2"].ToString().StartsWith(AcConst.UnderTrial)) || (Act["state1"].ToString().StartsWith(AcConst.UnderTrial) && Act["state2"].ToString().StartsWith(AcConst.Trial)))
            {
                // *提交和记账需要检查数据
                VaildateData(Data, Act);

                // 记账时设置对方科目单据体的科目为单据头的对方科目
                if (Act["state1"].ToString().StartsWith(AcConst.UnderTrial) && Act["state2"].ToString().StartsWith(AcConst.Trial))
                {
                    if (accountId > 0)
                    {
                        // 设置对方科目单据体的科目为单据头的对方科目
                        this.ApiTask.DB.ExecuteNonQuery($@"update voucherd set account={accountId},rver = rver + 1 where vh= {Data["id"].ToString()} and parent <= 0");

                        // 设置对方科目的科目性质为空
                        if (propertyId > 0)
                        {
                            // 设置对方科目的科目性质为空
                            this.ApiTask.DB.ExecuteNonQuery($@"update account set property=0,rver = rver + 1 where id= {accountId}");
                        }
                    }
                }
            }

            // 判断操作为反记账
            if (Act["state1"].ToString().StartsWith(AcConst.Trial) && Act["state2"].ToString().StartsWith(AcConst.Draft))
            {
                // 得到当前核算期
                var currentPeriod = PeriodHelper.GetCurrent(ApiTask);

                // 得到收/付款单的核算期
                int.TryParse(Data["period"]?.ToString(), out int period);

                // *检查单据的核算期是否小当前核算期
                if (period < currentPeriod)
                {
                    throw new Exception(this.ApiTask.LEnd("所选记录小于当前核算期，不允许反记"));
                }
            }

            // 设置是否流程更新标识
            Data["isapprove"] = true;
        }

        /// <summary>
        /// 单据审批后单据状态已经修改
        /// 1、提交后，清空记账人、记账时间，更新勾对记录
        /// 2、撤回后，清空记账人、记账时间，更新勾对记录
        /// 3、记账后，设置单据头的业务日期，设置存在父凭证的单据体的业务日期和到期日期，生成凭证，更新勾对记录，生成勾对记录
        /// 4、反记账后，清空记账人、记账时间，更新勾对记录，删除记账生成的凭证的勾对记录，删除记账生成的凭证体
        /// 注意：框架不管是“提交”还是“记账”等审批操作，都会设置记账人和记账时间
        /// </summary>
        /// <param name="Act">审批动作</param>
        /// <param name="id">当前单据头ID</param>
        protected override void OnApproveAfter(SData Act, int id)
        {
            // 根据id查询审批的单据
            var data = this.GetItem(id);

            // 判断为提交操作
            if (Act["state1"].ToString().StartsWith(AcConst.Draft) && Act["state2"].ToString().StartsWith(AcConst.UnderTrial) && data != null)
            {
                // *提交后清空记账人、记账时间，更新勾对记录
                // 清空记账人、记账时间
                data["poster"] = null;
                data["posttime"] = null;

                // 设置是否流程更新标识
                data["isapprove"] = true;

                // 更新勾对记录
                UpdateTickPair(data);
            }

            // 判断为撤回操作
            if (Act["state1"].ToString().StartsWith(AcConst.UnderTrial) && Act["state2"].ToString().StartsWith(AcConst.Draft) && data != null)
            {
                // *撤回后清空记账人、记账时间，更新勾对记录
                // 清空记账人、记账时间
                data["poster"] = null;
                data["posttime"] = null;

                // 设置是否流程更新标识
                data["isapprove"] = true;

                // 更新勾对记录
                UpdateTickPair(data);
            }

            // 判断为记账操作
            if (Act["state1"].ToString().StartsWith(AcConst.UnderTrial) && Act["state2"].ToString().StartsWith(AcConst.Trial) && data != null)
            {
                // *记账后设置单据头的业务日期，设置存在父凭证的单据体的业务日期和到期日期，生成凭证，更新勾对记录，生成勾对记录
                // 设置单据头的业务日期为当前日期
                data["bdate"] = DateTime.Now.ToString("yyyy-MM-dd");

                // 设置存在父凭证的单据体的业务日期和到期日期为当前日期
                this.ApiTask.DB.ExecuteNonQuery($@"update voucherd set bdate = '{DateTime.Now.ToString("yyyy-MM-dd")}',edate = '{DateTime.Now.ToString("yyyy-MM-dd")}',rver = rver + 1 where vh= {data["id"].ToString()} and parent > 0");

                if (accountId > 0)
                {
                    // 设置科目为收/付款科目
                    this.ApiTask.DB.ExecuteNonQuery($@"update voucherd set account=(select top 1 account from voucherd where vh = {data["id"].ToString()} and parent > 0),rver = rver + 1 where vh= {data["id"].ToString()} and parent <= 0");

                    if (propertyId > 0)
                    {
                        // 设置对方科目的科目性质为原来的
                        this.ApiTask.DB.ExecuteNonQuery($@"update account set property={propertyId},rver = rver + 1 where id= {accountId}");
                    }
                }

                // 生成凭证
                AddFinacialVoucher(data);

                // 更新勾对记录
                UpdateTickPair(data);

                // 生成勾对记录
                AddTickPair(data);
            }

            // 判断为反记账操作
            if (Act["state1"].ToString().StartsWith(AcConst.Trial) && Act["state2"].ToString().StartsWith(AcConst.Draft) && data != null)
            {
                // *反记账后，清空记账人、记账时间，更新勾对记录，删除记账生成的凭证的勾对记录，删除凭证
                // 清空记账人、记账时间
                data["poster"] = null;
                data["posttime"] = null;

                // 查询生成的凭证
                var voucher = ApiTask.Biz<BizTable>(nameof(AcPayRecV)).GetItemByParms(new SData("pvoucherid", data["id"].ToString()).toParmStr(), "id,vstate,vsubstate");

                // 判断生成的凭证存在，未被反记账，删除
                if (voucher != null)
                {
                    // 更新凭证的勾对状态
                    var query = ApiTask.Biz<BizQuery>(nameof(AcRecoD));
                    var recoData = ApiTask.Biz<BizTable>(nameof(AcRecoD)).GetItemByParms(new SData("recpay", data["id"].ToString()).toParmStr(), "batch");
                    AcVoucherHelper.UpdateRecoStateByDelete(query, recoData["batch"].ToString(), int.Parse(data["subrec.id"].ToString()));

                    // 更新勾对记录
                    UpdateTickPair(data);

                    // 删除记账生成的凭证的勾对记录
                    var sql = new StringBuilder();
                    sql.Append($@"delete a 
from voucherd as a 
where 
exists(select id 
    from voucherd as b 
    where
    b.vh = {voucher["id"]} and
    b.id = a.parent)");
                    this.ApiTask.DB.ExecuteNonQuery(sql.ToString());

                    // 如果凭证为已审，就反记账
                    var vhbiz = ApiTask.Biz<AcVoucher>(nameof(AcVoucher)); // 凭证头
                    var vdbiz = ApiTask.Biz<AcVoucherD>(nameof(AcVoucherD)); // 凭证体
                    var vdParm = new SData("vh", "[" + voucher["id"] + "]").toParmStr(); // 凭证体的过滤条件
                    if (voucher["vstate"].ToString().Sp_First() == AcConst.Trial)
                    {
                        // 修改子状态为正常
                        var vdList = vdbiz.GetListData(0, 1, vdParm, "id");
                        List<int> idList = vdList.Select(x => x.Item<int>("id")).ToList();
                        var rdList = this.GetDetail(id, BizD);
                        List<int> id1List = rdList.Select(x => x.Item<int>("id")).ToList();
                        AcVoucherHelper.UpdateVSubstateNoLock(this, idList, id1List);

                        // 反记账凭证
                        vhbiz.Approve(AcConst.AntAccounT, int.Parse(voucher["id"].ToString()));
                    }

                    // 删除记账生成的凭证
                    vdbiz.DeleteBatch(vdParm);
                    vhbiz.DeleteByID(int.Parse(voucher["id"].ToString()));
                }
            }

            // 设置是否流程更新标识
            if (data != null)
            {
                data["isapprove"] = true;
                this.Update(data);
            }
        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 提交、记账验证数据
        /// </summary>
        /// <param name="updateData">凭证头更新数据</param>
        /// <param name="act">审批动作</param>
        private void VaildateData(SData updateData, SData act)
        {
            // 格式校验
            this.ValidDataIsNullAndFormat(updateData);

            // 存在约束校验
            this.ValidDataExist(updateData);

            // 核算期校验
            PeriodHelper.Check(int.Parse(updateData["period"].ToString()), this.ApiTask);

            // 查询单据体
            var voucherDetailList = this.GetDetail(int.Parse(updateData["id"].ToString()), BizD);

            // 判断凭证体数据为空或者凭证体数量为0
            if (voucherDetailList == null || !voucherDetailList.Any())
            {
                throw new Exception(this.ApiTask.LEnd($"所选记录无{BizName}记录"));
            }

            // 设置修改时间为当前时间
            updateData["udate"] = DateTime.Now.ToString("yyyy-MM-dd");

            // 根据单据头科目对账查询要科目定义中的科目集合
            var subjectRecRelationList = ApiTask.Biz<BizTable>(nameof(AcReconRef)).GetList(0, 0, "subrec=" + updateData["subrec"], "subrec,account");

            // 查询对账科目定义的科目集合
            var subjectIds = subjectRecRelationList.Item<List<SData>>("data").ToList();

            // 除去对方科目
            var voucherDetailCheckList = voucherDetailList.Where(x => x["parent"]?.ToString() != "0");

            // 如果凭证分录不存在，抛出异常
            var detailCheckList = voucherDetailCheckList as SData[] ?? voucherDetailCheckList.ToArray();
            if (!detailCheckList.Any())
            {
                // 所选记录不存在
                throw new Exception(this.ApiTask.LEnd($"无{BizName}记录"));
            }

            // 检查tcode分析项是否一致不包括对方科目），抛出异常
            var tcodeList = AcVoucherHelper.GetRecTcodes(updateData["subrec"].ToString(), ApiTask); // 科目对账中启用的tcode
            foreach (var tcode in tcodeList)
            {
                if (detailCheckList.GroupBy(m => m[tcode]).Count() > 1 ||
                    string.IsNullOrEmpty(detailCheckList.FirstOrDefault().Item<string>(tcode)))
                {
                    throw new Exception(this.ApiTask.LEnd("分析项不相同"));
                }
            }

            // 分300/页 取凭证id
            var queryFields = "id,vstate,cd,amountd,amountc,subunit,account";
            var parentIdList = voucherDetailCheckList.Select(x => x["parent"].ToString()).ToList();
            var parentIds = AcVoucherHelper.GetIdsByPage(parentIdList);

            // 查询收付款对应的原始凭证体
            var voucherList = new List<SData>();
            foreach (var parentId in parentIds.Keys)
            {
                var pIds = string.Join(",", parentIds.Item<List<string>>(parentId).ToList());
                var list = ApiTask.Biz<BizTable>("AcVoucherD").GetListData(0, 1, new SData("id", pIds).toParmStr(), queryFields);
                voucherList.AddRange(list);
            }

            // 借方贷方金额合计变量
            decimal creditSum = 0, debitSum = 0;

            // 查询提审金额
            List<SData> amountpData = new List<SData>();

            // 查询已勾对金额
            List<SData> sumRecoamountData = new List<SData>();

            // 判断为记账操作，记账时才验证实收/实付金额是否合法
            if (act["state1"].ToString().StartsWith(AcConst.UnderTrial) &&
                act["state2"].ToString().StartsWith(AcConst.Trial))
            {
                // 科目对账
                var subRecid = updateData["subrec"].ToString();

                var queryRecFields = new List<string>() { "parent", "a_sum_recoamount" };
                var groupFields = new List<string>() { "parent" };

                // 获取提审金额
                // 分300/页 取凭证id
                foreach (var parentId in parentIds.Keys)
                {
                    var pIds = string.Join(",", parentIds.Item<List<string>>(parentId).ToList());
                    var sql = ApiTask.Biz<BizTable>("AcRecoD").QuerySql(new SData(AcConst.RecoTcode, subRecid, "account.dcode", AcConst.RecoAccount, "vstate", ":" + AcConst.UnderTrial, "parent", pIds), queryRecFields, groupFields, null, false);
                    var list = ApiTask.Biz<BizTable>("AcRecoD").ApiTask.DB.ExecuteList(sql).ToList();
                    amountpData.AddRange(list);
                }

                // 获取已勾对金额
                // 分300/页 取凭证id
                foreach (var parentId in parentIds.Keys)
                {
                    var pIds = string.Join(",", parentIds.Item<List<string>>(parentId).ToList());
                    var sql = ApiTask.Biz<BizTable>("AcRecoD").QuerySql(new SData(AcConst.RecoTcode, subRecid, "account.dcode", AcConst.RecoAccount, "vstate", AcConst.Trial, "parent", pIds), queryRecFields, groupFields, null, false);
                    var list = ApiTask.Biz<BizTable>("AcRecoD").ApiTask.DB.ExecuteList(sql).ToList();
                    sumRecoamountData.AddRange(list);
                }
            }

            foreach (var entity in voucherDetailList)
            {
                this.ValidateSubjectRelative(entity, subjectIds, act["state1"].ToString(), act["state2"].ToString(), voucherList, updateData, amountpData, sumRecoamountData);
                decimal.TryParse(entity["amountc"]?.ToString(), out decimal amountc);
                decimal.TryParse(entity["amountd"]?.ToString(), out decimal amountd);
                creditSum += amountc;
                debitSum += amountd;
            }

            // 判断借贷是否平衡
            if (creditSum != debitSum)
            {
                string amountMesg = BizName == "收款" ? "实收" : "实付";
                throw new Exception(this.ApiTask.LEnd($"对方科目金额和{amountMesg}金额不相等"));
            }

            // 检查原始核算单位是否一致
            if (!AcVoucherHelper.CheckSubUnitIsSame(voucherList))
            {
                throw new Exception(this.ApiTask.L("核算单位") + this.ApiTask.LEnd("不相同"));
            }
        }

        /// <summary>
        /// 校验与科目相关
        /// </summary>
        /// <param name="entity">收付款凭证体实体</param>
        /// <param name="subjectRelationList">对账定义中的科目集合</param>
        /// <param name="state1">流程开始状态</param>
        /// <param name="state2">流程结束状态</param>
        /// <param name="listVoucher">原始凭证列表</param>
        /// <param name="updateData">单据数据</param>
        /// <param name="amountpData">提审金额列表</param>
        /// <param name="sumRecoamountData">已勾对列表</param>
        public virtual void ValidateSubjectRelative(SData entity, List<SData> subjectRelationList, string state1, string state2, List<SData> listVoucher, SData updateData, List<SData> amountpData, List<SData> sumRecoamountData)
        {
            // 收/付款单体验证
            int.TryParse(entity["parent"].ToString(), out int parentId);
            if (parentId > 0)
            {
                // 获取凭证体
                var financialVoucherData = listVoucher.FirstOrDefault(x => x["id"]?.ToString() == parentId.ToString());

                // 如果凭证分录不存在，抛出异常
                if (financialVoucherData == null)
                {
                    // 所选记录不存在
                    throw new Exception(this.ApiTask.LEnd($"{BizName}凭证分录不存在"));
                }

                // 判断凭证是否是记账状态
                if (!financialVoucherData["vstate"].ToString().StartsWith(AcConst.Trial))
                {
                    throw new Exception(this.ApiTask.LEnd($"{BizName}凭证分录未审核"));
                }

                // 判断科目是否存在
                if (financialVoucherData["account"] == null)
                {
                    throw new Exception(this.ApiTask.LEnd("科目不存在"));
                }

                // 判断凭证分录的科目是否等于科目对账定义中的对账科目
                if (subjectRelationList == null || subjectRelationList.Count == 0)
                {
                    throw new Exception(this.ApiTask.LEnd($"{BizName}凭证分录的科目不属于对账科目"));
                }

                var subjectRelation = subjectRelationList.FirstOrDefault(s => s["account.id"]?.ToString() == financialVoucherData["account.id"].ToString());
                if (subjectRelation == null)
                {
                    throw new Exception(this.ApiTask.LEnd($"{BizName}凭证分录的科目不属于对账科目"));
                }

                string amountValue = BizD != null && BizD == "AcReceiptD" ? "amountd" : "amountc";

                int tagCd = BizD != null && BizD == "AcReceiptD" ? AcConst.Debit : AcConst.Credit;

                string title = BizD != null && BizD == "AcReceiptD" ? "贷方" : "借方";

                // 判断收款或者付款原始凭证的金额方向
                int.TryParse(financialVoucherData["cd"]?.ToString(), out int cd);
                if (cd != tagCd)
                {
                    throw new Exception(this.ApiTask.LEnd($"{BizName}凭证分录的金额在{title}不能{BizName}"));
                }

                // 判断实收/实付金额是否为0
                string amountMesg = BizName == "收款" ? "实收" : "实付";
                decimal.TryParse(entity[amountValue]?.ToString(), out decimal amount);
                if (amount == 0)
                {
                    throw new Exception(this.ApiTask.LEnd($"{amountMesg}金额不能为0"));
                }

                // 判断为记账操作，记账时才验证收付款金额是否合法
                if (state1.StartsWith(AcConst.UnderTrial) &&
                    state2.StartsWith(AcConst.Trial))
                {
                    // 得到已勾对金额
                    var recoamount = sumRecoamountData.FirstOrDefault(x => x["parent"]?.ToString() == parentId.ToString())?["a_sum_recoamount"];

                    decimal.TryParse(recoamount?.ToString(), out decimal tickPairAmount);

                    // 得到提审金额
                    var allAmountp = amountpData.FirstOrDefault(x => x["parent"]?.ToString() == parentId.ToString())?["a_sum_recoamount"];

                    decimal.TryParse(allAmountp?.ToString(), out decimal allAmount);

                    // 得到提审金额，全部提审金额减去本批次
                    decimal pendingAmount = allAmount - amount;

                    // 检查实收/实付金额是否大于应收或者应付-提审金额
                    CheckAmount(financialVoucherData, entity, tickPairAmount, pendingAmount);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(updateData.Item<string>("account")))
                {
                    string account = updateData["account"].ToString();

                    // 验证对方科目
                    CheckAll(entity, account);
                }

                if (!string.IsNullOrEmpty(updateData.Item<string>("account.id")))
                {
                    accountId = int.Parse(updateData["account.id"].ToString());
                    var account = ApiTask.Biz<BizTable>("acaccount").GetItem(accountId, "property.id");
                    if (account != null)
                    {
                        if (!string.IsNullOrEmpty(account.Item<string>("property.id")))
                        {
                            propertyId = int.Parse(account["property.id"].ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 验证数据必填和格式是否正确
        /// 1、必填：凭证类型、核算期、科目对账、数据类型
        /// 2、核算期格式
        /// 3、附件格式
        /// </summary>
        /// <param name="data">数据</param>
        private void ValidDataIsNullAndFormat(SData data)
        {
            // 校验必填（科目对账、凭证类型、核算期、对方科目）
            this.CheckEmptyOrNull(data, true, "subrec", "vtype", "period", "account");

            // 判断核算期格式是否正确
            this.CheckFormat("period", Regx.Period, data);

            this.CheckMaxLength("ddesc", 300, data);
        }

        /// <summary>
        /// 验证数据存在性：凭证类型、数据类型、状态、科目对账
        /// </summary>
        /// <param name="data">数据</param>
        private void ValidDataExist(SData data)
        {
            // 验证凭证类型、数据类型、状态、科目对账存在性
            this.CheckRefExist(data, "vtype", "vclass", "vstate", "subrec");
        }

        /// <summary>
        ///  添加凭证
        /// </summary>
        /// <param name="payRece">单据头数据</param>
        /// <returns>新增凭证头Id</returns>
        public int AddFinacialVoucher(SData payRece)
        {
            // 根据单据名称设置对方科目的科目代码
            string code = BizD == "AcReceiptD" ? "rec" : "pay";

            // 设置凭证数据
            SData financialVoucher = new SData
            {
                ["sumd"] = payRece["sumc"], // 借方合计
                ["sumc"] = payRece["sumd"], // 贷方合计
                ["vType"] = payRece["vType"], // 凭证类型
                ["maker"] = payRece["maker"], // 制单人
                ["mdate"] = payRece["mdate"], // 制单人
                ["ddesc"] = payRece["ddesc"], // 摘要
                ["title"] = payRece["title"], // 名称
                ["vclass"] = payRece["vclass"], // 数据类型
                ["period"] = payRece["period"], // 核算期
                ["SubUnit"] = payRece["SubUnit"], // 核算单位
                ["dcode"] = payRece["dcode"] + code, // 编号
                ["vstate"] = AcConst.Trial, // 状态：已审
                ["vsubstate"] = AcConst.LockS, // 子状态：锁定
                ["pvoucherid"] = payRece["id"], // 父id（头）
                ["poster"] = ApiTask.UserInfo().UserCode(), // 记账人
                ["bdate"] = DateTime.Now.ToString("yyyy-MM-dd"), // 业务日期
                ["udate"] = DateTime.Now.ToString("yyyy-MM-dd"), // 修改日期
                ["posttime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // 记账时间
            };

            // 添加凭证头
            var voucherId = this.ApiTask.Biz<BizTable>(nameof(AcPayRecV)).Insert(financialVoucher);

            // 添加凭证体列表
            var payReceDList = this.GetDetail(int.Parse(payRece["id"].ToString()), BizD); // 获取单据体集合
            foreach (var payReceiptD in payReceDList)
            {
                // 声明凭证体
                SData entity = new SData
                {
                    ["qty"] = payReceiptD["qty"], // 数量
                    ["price"] = payReceiptD["price"], // 单价
                    ["crate"] = payReceiptD["crate"], // 汇率
                    ["camount"] = payReceiptD["camount"], // 原币
                    ["currency"] = payReceiptD["currency"], // 币种

                    ["vstate"] = AcConst.Trial, // 状态：已审
                    ["vsubstate"] = AcConst.LockS, // 子状态：锁定
                    ["ddesc"] = payReceiptD["ddesc"], // 摘要
                    ["vh"] = "[" + voucherId + "]", // 凭证头Id
                    ["vclass"] = payRece["vclass"], // 数据类型
                    ["period"] = payReceiptD["period"], // 核算期
                    ["SubUnit"] = payRece["SubUnit"], // 核算单位

                    ["unitname"] = payReceiptD["unitname"], // 计量单位
                    ["reference"] = payReceiptD["reference"], // 业务参考
                    ["parent"] = payReceiptD["id"], // 父id：收/付款体的id
                    ["amountc"] = payReceiptD["amountd"], // 贷方金额：收/付款单的借方金额
                    ["amountd"] = payReceiptD["amountc"], // 借方金额：收/付款单的贷方金额
                };

                // 是否存在父Id
                int.TryParse(payReceiptD["parent"].ToString(), out int parentId);
                if (parentId > 0)
                {
                    // 获取父凭证体，设置业务日期、到期日期、科目
                    var voucherd = ApiTask.Biz<BizTable>(nameof(AcVoucherD)).GetItem(parentId, "account");
                    if (voucherd != null)
                    {
                        entity["bdate"] = DateTime.Now.ToString("yyyy-MM-dd"); // 设置业务日期
                        entity["edate"] = DateTime.Now.ToString("yyyy-MM-dd"); // 设置到期日期
                        entity["account"] = voucherd["account"]?.ToString(); // 设置科目为父凭证的科目
                    }
                }
                else
                {
                    entity["account"] = payRece["account"]?.ToString(); // 科目直接取单据头的科目id
                    entity["bdate"] = payReceiptD["bdate"]; // 设置业务日期为对方科目单据体的业务日期
                    entity["edate"] = payReceiptD["edate"]; // 设置到期日期为对方科目单据体的到期日期
                }

                decimal.TryParse(entity["amountd"]?.ToString(), out decimal amountd);
                decimal.TryParse(entity["amountc"]?.ToString(), out decimal amountc);

                // 设置借贷方向,0：借，1：贷
                entity["cd"] = payReceiptD["cd"].ToString() == AcConst.Credit.ToString() ? AcConst.Debit : AcConst.Credit;

                // 设置金额
                entity["amount"] = amountd - amountc;

                // 设置tcode
                string matchTCode = Regx.TcodeModel;
                var props = payReceiptD.Where(x => Regex.IsMatch(x.Key, matchTCode));
                foreach (var prop in props)
                {
                    entity[prop.Key] = payReceiptD[prop.Key];
                }

                // 添加凭证体
                this.ApiTask.Biz<BizTableVoucherD>(nameof(AcVoucherD)).Insert(entity);
            }

            return voucherId;
        }

        /// <summary>
        /// 添加勾对记录，修改勾对记录状态
        /// </summary>
        /// <param name="voucherData">单据数据</param>
        public void AddTickPair(SData voucherData)
        {
            // 获取单据体集合
            var receivableVoucherDetailList = ApiTask.Biz<BizTable>(BizD).GetListData(0, 1, "vh=[" + voucherData["id"].ToString() + "]", "parent,id,cd");
            List<SData> tickPairDetailList = new List<SData>();
            var recoDList = new SData();

            int i = 0; // 计数

            // 添加勾对体
            foreach (var item in receivableVoucherDetailList)
            {
                // 有父凭证Id
                int.TryParse(item["parent"]?.ToString(), out int parent);
                if (parent > 0)
                {
                    string amountValue = BizD != null && BizD == "AcReceiptD" ? "amountc" : "amountd";

                    // 根据单据的父Id查询要添加勾对记录的凭证
                    var data = ApiTask.Biz<BizTable>(nameof(AcVoucherD)).GetItemByParms("parent=" + item["id"], "cd,id,amountc,amountd");

                    decimal.TryParse(data[amountValue]?.ToString(), out var tickPairAmount);

                    // 第一个调勾对记录数据赋值
                    if (i == 0)
                    {
                        recoDList = new SData("recoamount", tickPairAmount, AcConst.RecoTcode, $"[{voucherData["subrec.id"]}]", "cd", data["cd"], "logicsign", AcConst.RecoVoucher, "parent", data["id"], "recpay", voucherData["id"].ToString());
                    }

                    // 除第一条外的勾对记录数据赋值
                    else
                    {
                        // 设置勾对金额，对账科目，借贷标识，勾对类型为单据勾对，凭证父Id
                        tickPairDetailList.Add(new SData("recoamount", tickPairAmount, AcConst.RecoTcode, $"[{voucherData["subrec.id"]}]", "cd", data["cd"], "logicsign", AcConst.RecoVoucher, "parent", data["id"], "recpay", voucherData["id"].ToString()));
                    }

                    i++;
                }
            }

            // 后面的勾对记录作为一个recod值存入第一条勾对记录
            if (i > 1)
            {
                recoDList["recod"] = tickPairDetailList;
            }

            // 添加勾对记录
            int id = ApiTask.Biz<BizTable>(nameof(AcRecoD)).Insert(recoDList);
            var recoData = ApiTask.Biz<BizTable>(nameof(AcRecoD)).GetItemByParms(new SData("id", id).toParmStr(), "batch,bdate,recoperson");

            // 设置勾对记录的批次、勾对日期、勾对人一致
            this.ApiTask.DB.ExecuteNonQuery($@"update voucherd set s0='{recoData["batch"]}',bdate='{recoData["bdate"]}',a7={recoData["recoperson.id", 0]},rver = rver + 1 where a6 = {voucherData["id"]} ");

            // 更新凭证的勾对状态
            var query = ApiTask.Biz<BizQuery>(nameof(AcRecoD));
            AcVoucherHelper.UpdateRecoState(query, recoData["batch"].ToString(), int.Parse(voucherData["subrec.id"].ToString()));
        }

        /// <summary>
        ///  检查实收/实付金额是否合法
        /// </summary>
        /// <param name="financialVoucherDetailData">凭证体数据</param>
        /// <param name="data">单据体数据</param>
        /// <param name="tickPairAmount">已经勾对金额</param>
        /// <param name="pendingAmount">提审金额</param>
        public void CheckAmount(SData financialVoucherDetailData, SData data, decimal tickPairAmount, decimal pendingAmount)
        {
            string amountValue = BizD != null && BizD == "AcReceiptD" ? "amountd" : "amountc";

            string title = BizD != null && BizD == "AcReceiptD" ? "借方" : "贷方";

            // 判断收款或者付款金额是否为0
            decimal.TryParse(data[amountValue]?.ToString(), out decimal amount);

            // 如果收款（付款）金额大于可收（可付）款金额（借方(贷方)金额 - 已勾对金额 - 提审金额），抛出异常
            decimal.TryParse(financialVoucherDetailData[amountValue]?.ToString(), out decimal allAmount);
            decimal canReceivableAmount = allAmount - tickPairAmount - pendingAmount;
            if (Math.Abs(amount) > Math.Abs(canReceivableAmount))
            {
                // 收款（付款）金额不能大于【借方(贷方)金额 - 已勾对金额 - 提审金额】
                string amountMesg = BizName == "收款" ? "实收" : "实付";
                throw new Exception(this.ApiTask.LEnd($"{amountMesg}金额不能大于【{title}金额 - 已勾对金额 - 提审金额】"));
            }
        }

        /// <summary>
        /// 校验对方科目
        /// 1、如果科目没启用：科目已禁用，不允许使用
        /// 2、科目的数量为启用时，如果数量不合法，抛出异常
        /// 3、科目的外币为启用时，如果币种、汇率不合法，抛出异常
        /// 4、判断单据体中 科目、类定义启用的tcode属性值是否合法
        /// </summary>
        /// <param name="entity">对方科目单据体</param>
        /// <param name="account">科目代码</param>
        private void CheckAll(SData entity, string account)
        {
            // 错误消息
            List<string> message = new List<string>();

            // 得到科目对象
            var accountBiz = this.ApiTask.Biz<BizTable>("AcAccount");
            SData subject = accountBiz.GetItemByParms(new SData("dcode", account).toParmStr(), "id,isenable,isqty,iscurrency,currency," + AcVoucherHelper.TcodeStr);
            if (subject != null)
            {
                // *如果科目没启用，抛出异常
                if (!subject.Item<bool>("isenable"))
                {
                    message.Add(this.ApiTask.LEnd("科目已禁用，不允许使用"));
                }

                var bizD = this.ApiTask.Biz<BizTable>(BizD);

                SData listField = bizD.ListField;

                // *科目的数量为启用时，判断数量是否合法
                if (subject.Item<bool>("isqty"))
                {
                    // 如果汇率为0，抛出异常
                    if (decimal.Parse(entity["qty"].ToString()) == 0)
                    {
                        message.Add(this.ApiTask.L(((FieldDefine)listField["qty"]).DisplayName) + this.ApiTask.LEnd("不能为0"));
                    }
                }

                // *科目的币种为启用时，判断币种、汇率是否合法
                if (subject.Item<bool>("iscurrency"))
                {
                    // 如果科目没有指定币种，币种为空时，抛出异常
                    if (string.IsNullOrEmpty(subject.Item<string>("currency")))
                    {
                        if (string.IsNullOrEmpty(entity.Item<string>("currency")))
                        {
                            message.Add(((FieldDefine)listField["currency"]).DisplayName + this.ApiTask.LEnd(PubLang.NotEmpty));
                        }
                    }

                    // 如果汇率为0，抛出异常
                    if (decimal.Parse(entity["crate"].ToString()) == 0)
                    {
                        message.Add(((FieldDefine)listField["crate"]).DisplayName + this.ApiTask.LEnd("不能为0"));
                    }

                    // 如果科目没有指定币种，币种不存在，抛出异常
                    if (string.IsNullOrEmpty(subject.Item<string>("currency")))
                    {
                        if (!string.IsNullOrEmpty(entity.Item<string>("currency")))
                        {
                            this.CheckRefExist(entity["currency"].ToString(), (FieldDefine)listField["currency"]);
                        }
                    }
                }

                // *判断单据体中 科目、类定义启用的tcode属性值是否合法
                var messageTcode = AcVoucherHelper.CheckTcode(ApiTask, subject, entity, true, false).ToList();

                message.AddRange(messageTcode);

                // 错误消息记录大于0，抛出错误异常
                if (message.Count > 0)
                {
                    throw new Exception("\n" + string.Join("\r", message));
                }
            }
        }

        /// <summary>
        /// 更新勾对记录
        /// </summary>
        /// <param name="voucherData">单据数据</param>
        public void UpdateTickPair(SData voucherData)
        {
            // 更新勾对记录状态和单据一致
            this.ApiTask.DB.ExecuteNonQuery($@"update voucherd set vstate={voucherData["vstate.id"]},rver = rver + 1 where a6 = {voucherData["id"]} ");
        }

        #endregion
    }
}
