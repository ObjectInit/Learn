#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcRecoState.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：勾对状态
* 创 建 者：曾倩倩
* 创建时间：2019/9/4 11:04:07
* =============================================================*/
#endregion

using SJ.Global;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 勾对状态
    /// 1 描述：无
    /// 2 约定：z1.从未勾对；z2.部分勾对；z3.未勾对；z4.全部勾对；z5.已勾对
    /// 3 业务逻辑：无
    /// </summary>
    [ClassData("cname", "勾对状态")]
    public class AcRecoState : Pub.PubOthers
    {
    }
}
