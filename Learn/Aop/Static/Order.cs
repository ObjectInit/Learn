#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Order.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Order 
* 创 建 者：Administrator 
* 创建时间：2019/5/16 15:46:01 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Aop.Static
{
    /// <summary>
    /// 需要处理的Order类信息
    /// </summary>
    public class Order
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public int Count { set; get; }
        public double Price { set; get; }
        public string Desc { set; get; }
    }
}
