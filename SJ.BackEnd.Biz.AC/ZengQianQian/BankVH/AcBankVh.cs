#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcBankVh.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：银行对账单头
* 创 建 者：曾倩倩
* 创建时间：2020/4/30 10:06:24
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.Global;
using System;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 银行对账单头
    /// 1 描述：无
    /// 2 约定：无
    /// 3 业务逻辑：
    ///     新增前：初始化数据
    /// </summary>
    [ClassData("cname", "银行对账单头", "vision", 1)]
    public class AcBankVh : AcVoucher
    {
        public override string DataClass => "AcVoucher";

        /// <summary>
        /// 预留字段、参数、子查询定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // 字段定义 **********
            // 更改显示名称
            ((FieldDefine)ListField["period"]).DisplayName = this.ApiTask.L("核算期");
            ((FieldDefine)ListField["subunit"]).DisplayName = this.ApiTask.L("核算单位");

            // 删除不需要的字段
            this.ListField.Remove("attach"); // 附件张数
        }

        #region 框架方法重写

        /// <summary>
        /// 新增前
        /// 初始化数据
        /// </summary>
        /// <param name="data">实体数据</param>
        protected override void OnInsertBefore(SData data)
        {
            // * 初始化数据
            this.InitData(data);
        }

        /// <summary>
        /// 屏蔽基类
        /// </summary>
        /// <param name="data">实体数据</param>
        protected override void OnInsertAfter(SData data)
        {
        }

        /// <summary>
        /// 屏蔽基类
        /// </summary>
        /// <param name="data">实体数据</param>
        protected override void OnDeleteBefore(SData data)
        {
        }

        /// <summary>
        /// 屏蔽基类
        /// </summary>
        /// <param name="data">实体数据</param>
        protected override void OnDeleteAfter(SData data)
        {
        }

        /// <summary>
        /// 屏蔽基类
        /// </summary>
        /// <param name="oldData">实体原始数据</param>
        /// <param name="updateData">实体修改数据</param>
        protected override void OnUpdateBefore(SData oldData, SData updateData)
        {
        }

        /// <summary>
        /// 屏蔽基类
        /// </summary>
        /// <param name="oldData">实体原始数据</param>
        /// <param name="updateData">实体修改数据</param>
        /// <param name="modify">修改属性</param>
        protected override void OnUpdateAfter(SData oldData, SData updateData, SData modify)
        {
        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="bank">银行对账单头</param>
        private void InitData(SData bank)
        {
            bank["dcode"] = VoucherHelper.GetMaxBankCode(this.ApiTask, "voucher", "mdate", "dcode", "bkcode");
            bank["title"] = "银行对账单"; // 名称
            bank["vstate"] = AcConst.Trial; // 单据状态
            bank["vsubstate"] = AcConst.LockS; // 子状态：锁定
            bank["vclass"] = AcConst.ActData; // 数据类型
            bank["vtype"] = AcConst.BankVhType; // 凭证类型
            bank["mdate"] = DateTime.Now.ToString("yyyy-MM-dd"); // 制单日期：当天日期
            bank["maker"] = $"[{this.ApiTask.UserInfo().UserID()}]"; // 制单人：当前登录用户
            bank["bdate"] = bank["bdate", DateTime.Now.ToString("yyyy-MM-dd")]; // 业务日期：当天日期
        }

        #endregion
    }
}
