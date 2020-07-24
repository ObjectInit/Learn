#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcAnalyType.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：账龄分析类型
* 创 建 者：曾倩倩
* 创建时间：2019/9/4 11:04:36
* =============================================================*/
#endregion

using SJ.Global;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 账龄分析类型
    /// 1 描述：无
    /// 2 约定：z1.将要到期；z2.逾期
    /// 3 业务逻辑：无
    /// </summary>
    [ClassData("cname", "账龄分析类型")]
    public class AcAnalyType : Pub.PubOthers
    {
    }
}
