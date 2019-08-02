#region  <<版本注释>>
/* ========================================================== 
// <copyright file="User.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：User 
* 创 建 者：Administrator 
* 创建时间：2019/8/2 13:27:07 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Console.AutoMapper
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public int? BookCount { get; set; }
    }
}
