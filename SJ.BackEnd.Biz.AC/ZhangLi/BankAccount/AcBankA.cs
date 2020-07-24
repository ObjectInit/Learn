#region  <<版本注释>>
/* ========================================================== 
// <copyright file="AcBankA.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：银行对账 
* 创 建 者：张莉
* 创建时间：2020/4/30 10:01:44
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 银行对账
    /// 1 描述：无
    /// 2 约定：修改时使用initbk标识“确定初始化”功能
    /// 3 业务逻辑：
    ///     修改前：
    ///         1、核算单位、银行科目、银行账户不允许修改
    ///         2、确定初始化
    ///     修改后：
    ///         1、初始化后添加银行对账单科目、银行对账单
    /// </summary>
    [ClassData("cname", "银行对账", "vision", 1)]
    public class AcBankA : PubOthers
    {
        /// <summary>
        /// 字段定义、查询参数定义、子查询定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // * 字段定义
            // 启用预留字段
            this.AddField("subunit", ApiTask.L("核算单位"), EFieldType.关联, "AcSubunit", "a9");
            this.AddField("bkaccount", ApiTask.L("银行科目"), EFieldType.关联, "AcAccount", "a8");
            this.AddField("bank", ApiTask.L("银行账户"), EFieldType.关联, "AcBank", "a7");

            this.AddField("bkamount", ApiTask.L("对账单期初余额"), EFieldType.数值, string.Empty, "d0");
        }

        #region 框架方法重写

        /// <summary>
        /// 银行对账新增前处理的逻辑：
        /// 1、初始化新增数据
        /// 2、数据格式验证
        /// 3、存在性验证
        /// 4、逻辑验证
        /// </summary>
        /// <param name="data">银行对账实体</param>
        protected override void OnInsertBefore(SData data)
        {
            if (data != null)
            {
                // 参数定义 **********
                // * 初始化新增数据
                data["dcode"] = Guid.NewGuid().ToString();
                data["title"] = "银行对账";

                // 判断逻辑 **********
                // * 数据格式验证
                // 校验必填
                this.CheckEmptyOrNull(data, true, "subunit", "bkaccount", "bank");
                this.CheckMaxLength("ddesc", 300, data);

                // 期初余额校验（可为空，可以为0，是数值）
                var bkamount = data.Item<string>("bkamount");
                if (!string.IsNullOrEmpty(bkamount))
                {
                    AcVoucherHelper.CheckDecimal(this.ApiTask, bkamount, ((FieldDefine)ListField["bkamount"]).DisplayName, false);
                }

                // 业务逻辑 **********
                // * 存在性验证：核算单位、银行科目、银行账户
                this.CheckRefExist(data, "subunit", "bkaccount", "bank");

                // * 逻辑验证
                // 银行科目已存在 且 银行科目未启用【银行账户Tcode】，提示：银行科目未启用银行账户辅助项，不允许重复新增。
                var count = this.GetCount($"bkaccount={data["bkaccount"]}");
                var accountId = this.ApiTask.BizTableCode("AcAccount").GetIDAuto(data["bkaccount"].ToString());
                var tcode = AcVoucherHelper.GetBCTcode(accountId.ToString(), ApiTask);
                if (count > 0 && tcode.Count == 0)
                {
                    throw new Exception($"{ApiTask.LEnd("银行科目未启用银行账户辅助项，不允许重复新增")}");
                }

                // 已存在银行对账中，提示：银行账户已关联银行科目。
                var countByBank = this.GetCount($"bank={data["bank"]}");
                if (countByBank > 0)
                {
                    throw new Exception($"{ApiTask.LEnd("银行账户已关联银行科目")}");
                }

                base.OnInsertBefore(data);
            }
        }

        /// <summary>
        /// 银行对账修改前处理的逻辑：
        /// 1、核算单位、银行科目、银行账户不允许修改
        /// 2、确定初始化
        /// </summary>
        /// <param name="oldData">修改前数据</param>
        /// <param name="updateData">修改后数据</param>
        protected override void OnUpdateBefore(SData oldData, SData updateData)
        {
            // 业务逻辑 **********
            // * 核算单位、银行科目、银行账户不允许修改
            updateData.Remove("subunit");
            updateData.Remove("bkaccount");
            updateData.Remove("bank");

            this.CheckMaxLength("ddesc", 300, updateData);

            // 期初余额校验（可为空，可以为0，是数值）
            var bkamount = updateData.Item<string>("bkamount");
            if (!string.IsNullOrEmpty(bkamount))
            {
                AcVoucherHelper.CheckDecimal(this.ApiTask, bkamount, ((FieldDefine)ListField["bkamount"]).DisplayName, false);

                // 当银行账户有不等于核算期190001的对账单，不修改对账单期初余额
                var bankVDfilterParm = new SData("period", $"!{AcConst.PeriodBegin}");

                // 是否存在银行对账单
                if (ExistBankVD(oldData, bankVDfilterParm))
                {
                    updateData.Remove("bkamount");
                }
            }

            base.OnUpdateBefore(oldData, updateData);
        }

        /// <summary>
        /// 银行对账查询前处理的逻辑：
        /// 1、按照银行科目代码升序排序
        /// </summary>
        /// <param name="qParm">查询参数</param>
        protected override void OnGetListBefore(SData qParm)
        {
            base.OnGetListBefore(qParm);

            // 业务逻辑 **********
            // * 按照银行科目代码升序排序
            if (string.IsNullOrEmpty(qParm.Item<string>("q.orderby")))
            {
                qParm.Append("q.orderby", "bkaccount.dcode");
            }
        }

        /// <summary>
        /// 删除前校验数据
        /// 1、银行对账导入对账单不允许删除
        /// </summary>
        /// <param name="data">对象</param>
        protected override void OnDeleteBefore(SData data)
        {
            // 业务逻辑 **********
            // * 银行对账导入对账单不允许删除，提示：银行对账已导入对账单，不允许删除。
            var banka = this.GetItem(data.Item<int>("id"), "id,subunit.id,bkaccount.id,bank.id");
            if (banka?.Count > 0)
            {
                // 是否存在银行对账单
                if (ExistBankVD(banka, new SData()))
                {
                    throw new Exception($"{ApiTask.LEnd("银行对账已导入对账单，不允许删除")}");
                }
            }

            base.OnDeleteBefore(data);
        }
        #endregion

        #region 自定义方法

        /// <summary>
        /// 是否存在银行对账单
        /// </summary>
        /// <param name="banka">银行对账</param>
        /// <param name="bankVDfilterParm">银行对账单查询参数</param>
        /// <returns>true存在，false不存在</returns>
        private bool ExistBankVD(SData banka, SData bankVDfilterParm)
        {
            // 银行对账单-添加过滤条件
            bankVDfilterParm.Append("subunit", $"[{banka["subunit.id"]}]");
            bankVDfilterParm.Append("bkaccount", $"[{banka["bkaccount.id"]}]");
            bankVDfilterParm.Append("bank", $"[{banka["bank.id"]}]");
            bankVDfilterParm.Append("vh.vtype", AcConst.BankVhType); // 筛选凭证类型为z0.对账单的银行对账单
            var bankVdCount = this.ApiTask.Biz<BizTable>(nameof(AcBankVD)).GetCount(bankVDfilterParm.toParmStr());
            return bankVdCount > 0;
        }

        #endregion

    }
}
