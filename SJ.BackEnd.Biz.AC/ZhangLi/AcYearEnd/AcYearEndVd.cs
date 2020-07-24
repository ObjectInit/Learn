#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcYearEndVd.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcYearEndVd
* 创 建 者：张莉
* 创建时间：2019/9/30 9:24:39
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.Global;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 结转凭证体说明：
    /// 1 描述：期末结账时将某一账户的余额或差额转入另一科目
    /// 2 约定：继承AcVoucherD，dclass是0
    /// </summary>
    [ClassData("cname", "自动凭证结转凭证体", "vision", 1)]
    public class AcYearEndVd : AcVoucherD
    {
        /// <summary>
        /// 字段定义、查询参数定义、子查询定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // 更改显示名称
            ((FieldDefine)ListField["ddesc"]).DisplayName = ApiTask.L("摘要");
        }
    }
}
