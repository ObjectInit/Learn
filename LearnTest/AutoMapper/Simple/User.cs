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

namespace Learn.Console.AutoMapper.Simple
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int age { get; set; }

        public int? BookCount { get; set; }

        public int MyAge { get; set; }

        public A MyExt { get; set; }

        public string IgnoreName { get; set; }

        public int Gender { get; set; }
    }
}
