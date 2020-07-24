#region  <<版本注释>>
/* ========================================================== 
// <copyright file="AcPayRecV.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcPayRecV 
* 创 建 者：李琳
* 创建时间：2019/10/31 13:19:52 
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.Global;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 记账产生的凭证头
    /// 1 描述：收付款单记账产生的凭证头
    /// 2 约定：定义预留字段a9为pvoucherid，字段存收付款单据的id；dclass是acvoucher
    /// </summary>
    [ClassData("cname", "收付款记账产生凭证头", "vision", 1)]
    public class AcPayRecV : AcVoucher
    {
        /// <summary>
        /// 收付款单记账生成的凭证头共用凭证dclass
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
            this.AddField("pvoucherid", "单据id", EFieldType.数值, string.Empty, "a9");
        }
    }
}
