#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2019/9/10 10:34:02 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Console.AutoMapper.Unflattening
{
    public class Order
    {
        public decimal Total { get; set; }
        public Customer Customer { get; set; }
    }

    public class OrderDto
    {
        public decimal Total { get; set; }
        public string CustomerName { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; }
    }

    class Program:IMain
    {
        public void Main(string[] args)
        {
            SimpleReverse();
        }

        /// <summary>
        /// 简单的反向映射示例
        /// </summary>
        private void SimpleReverse()
        {

        }
    }
}
