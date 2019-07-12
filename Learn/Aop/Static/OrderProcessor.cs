#region  <<版本注释>>
/* ========================================================== 
// <copyright file="OrderProcessor.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：OrderProcessor 
* 创 建 者：Administrator 
* 创建时间：2019/5/16 15:47:03 
* =============================================================*/
#endregion

using System;

namespace Learn.UnitTest.Aop.Static
{
    /// <summary>
    /// 自定义实现提交
    /// </summary>
    public class OrderProcessor:IOrderProcessor
    {
        public void Submit(Order order)
        {
            Console.WriteLine("提交订单");
        }
    }
}
