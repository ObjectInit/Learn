#region  <<版本注释>>
/* ========================================================== 
// <copyright file="OrderProcessorDecorator.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：OrderProcessorDecorator 
* 创 建 者：Administrator 
* 创建时间：2019/5/16 16:46:36 
* =============================================================*/
#endregion

using System;

namespace Learn.UnitTest.Aop.Static
{
    /// <summary>
    /// 系统级别的提交模拟
    /// </summary>
    public class OrderProcessorDecorator:IOrderProcessor
    {
        public IOrderProcessor OrderProcessor { get; set; }

        /// <summary>
        /// 可通过依赖注入实现
        /// </summary>
        /// <param name="orderprocessor"></param>
        public OrderProcessorDecorator(IOrderProcessor orderprocessor)
        {
            OrderProcessor = orderprocessor;
        }
        public void PreProceed(Order order)
        {
            Console.WriteLine("提交订单前，进行订单数据校验....");
            if (order.Price < 0)
            {
                Console.WriteLine("订单总价有误，请重新核对订单。");
            }
        }

        public void PostProceed(Order order)
        {
            Console.WriteLine("提交带单后，进行订单日志记录......");
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "提交订单，订单名称：" + order.Name + "，订单价格：" + order.Price);
        }
        /// <summary>
        /// 系统级别的提交订单被划分为很多个前置或者后置任务
        /// </summary>
        /// <param name="order"></param>
        public void Submit(Order order)
        {

            PreProceed(order);//前置
            OrderProcessor.Submit(order);//任务
            PostProceed(order);//后置
        }
    }
}
