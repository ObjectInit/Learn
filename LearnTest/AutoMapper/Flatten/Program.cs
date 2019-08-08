#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2019/8/2 17:20:58 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Learn.Console.AutoMapper.Flatten
{
    /// <summary>
    /// 这是一个描述的复杂对象
    /// 包含客户实体、订单项、添加订单项、和获取订单总价的方法
    /// </summary>
    public class Order
    {
        private readonly IList<OrderLineItem> _orderLineItems = new List<OrderLineItem>();

        public Customer Customer { get; set; }

        public OrderLineItem[] GetOrderLineItems()
        {
            return _orderLineItems.ToArray();
        }

        public void AddOrderLineItem(Product product, int quantity)
        {
            _orderLineItems.Add(new OrderLineItem(product, quantity));
        }

        public decimal GetTotal()
        {
            return _orderLineItems.Sum(li => li.GetTotal());
        }
    }

    /// <summary>
    /// 这是需要转换的简单实体
    /// 只包含了客户的名称和订单总价
    /// </summary>
    public class OrderDto
    {
        public string CustomerName { get; set; }
        public decimal Total { get; set; }
    }

    public class Product
    {
        public decimal Price { get; set; }
        public string Name { get; set; }
    }

    public class OrderLineItem
    {
        public OrderLineItem(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
        }

        public Product Product { get; private set; }
        public int Quantity { get; private set; }

        public decimal GetTotal()
        {
            return Quantity * Product.Price;
        }
    }

    public class Customer
    {
        public string Name { get; set; }
    }


    public class Program:IMain
    {
        public void Main(string[] args)
        {
            //auto mapper 如何 映射这种复杂到简单的实体
            Mapper.Initialize(cif => cif.CreateMap<Order, OrderDto>());

            //初始化各种数据 并查看auto mapper 如何约定数据映射
            var customer = new Customer
            {
                Name = "George Costanza"
            };
            var order = new Order
            {
                Customer = customer
            };
            var bosco = new Product
            {
                Name = "Bosco",
                Price = 4.99m
            };
            order.AddOrderLineItem(bosco, 15);
            var orderDto= Mapper.Map<Order, OrderDto>(order);
        }
    }
}
