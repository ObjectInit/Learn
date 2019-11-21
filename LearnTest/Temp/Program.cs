#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2019/7/2 13:13:03 
* =============================================================*/
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using AutoMapper;
using System.Linq;
namespace Learn.Console.Temp
{
    public class User
    {
        public string Name { get; set; }

        public int age { get; set; }

    }

    public class UserDto
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }
    public class Program : IMain
    {
        public void Main(string[] args)
        {
            //1.创建mapper对象,通过MapperConfiguration对象创建
            MapperConfiguration config = new MapperConfiguration(
                cfg => cfg.CreateMap<User, UserDto>()
            );
            var mapper = config.CreateMapper();
        } 
    } 
}
