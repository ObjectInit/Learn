#region  <<版本注释>>
/* ========================================================== 
// <copyright file="UserDto.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：UserDto 
* 创 建 者：Administrator 
* 创建时间：2019/8/2 13:27:35 
* =============================================================*/
#endregion

using System;

namespace Learn.Console.AutoMapper.Simple
{
    public class UserDto
    {
        public int Id { get; set; }

        public string name { get; set; }

        public int Age { get; set; }

        public int? BookCount { get; set; }

        public string SimpleName { get; set; }

        public string MyAge { get; set; }

        public B MyExt { get; set; }

        public string IgnoreName { get; set; }

        public string NoneProper { get; set; }

        public string Gender { get; set; }
    }
}
