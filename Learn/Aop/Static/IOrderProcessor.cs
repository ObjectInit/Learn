#region  <<版本注释>>
/* ========================================================== 
// <copyright file="IOrderProcessor.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：IOrderProcessor 
* 创 建 者：Administrator 
* 创建时间：2019/5/16 15:45:17 
* =============================================================*/
#endregion

namespace Learn.UnitTest.Aop.Static
{
    /// <summary>
    /// 接口规范 提供提交订单接口
    /// </summary>
    public interface IOrderProcessor
    {
        void Submit(Order order);
    }
}
