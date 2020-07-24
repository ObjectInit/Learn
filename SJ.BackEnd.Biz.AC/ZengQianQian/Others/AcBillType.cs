#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcBillType.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：单据类型
* 创 建 者：曾倩倩
* 创建时间：2019/9/4 11:05:05
* =============================================================*/
#endregion

using SJ.Global;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 单据类型
    /// 1 描述：无
    /// 2 约定：zsk.收款单；zfk.付款单
    /// 3 业务逻辑：无
    /// </summary>
    [ClassData("cname", "单据类型")]
    public class AcBillType : Pub.PubOthers
    {
    }
}
