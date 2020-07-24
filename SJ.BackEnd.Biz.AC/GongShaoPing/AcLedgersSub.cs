#region  << 版 本 注 释 >>
/* ============================================================================== 
// <copyright file="AcLedgersSub.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcLedgersSub 
* 创 建 者：龚绍平
* 创建时间：2019/10/31 14:57:46 
* ==============================================================================*/
#endregion
using SJ.BackEnd.Base;
using SJ.Global;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 明细账子对象
    /// 1 描述: 为实现a9（科目对账）查询功能建立的继承框架明细账的对象
    /// 2 约定：无
    /// 3 业务逻辑：无
    /// </summary>
    [ClassData("cname", "明细账子对象")]
    public class AcLedgersSub : AcLedgers
    {
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // 科目对账
            //this.AddField("subrec", "科目对账", EFieldType.关联, "AcSubRec", "a9");
            AcVoucherHelper.AddRecoField(this);
            // 查询参数
            this.AddParm("payreceipt", "收付款");
            
            // 子查询定义 **********
            // 凭证体子查询
            this.AddSubQuery("voucherd", "凭证分录", "AcVoucherD", "id=parent");
        }

        protected override string CustomParmSql(string parmKey, string parmStr)
        {
            var sql = base.CustomParmSql(parmKey, parmStr);

            // 收付款
            if (parmKey.ToLower() == "payreceipt")
            {
                sql += $@"
exists (select 1
    from
    voucherd
    where
    vh={parmStr} and
    parent={{0}}.id )";
            }

            return sql;
        }
    }
}