﻿#region  <<版本注释>>
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
    public class Program : IMain
    {
        public void Main(string[] args)
        {
            List<int> a = new List<int> {1, 2};
            var newA = a.Union(new List<int> {2, 3});
        }
    }

    public class A:B
    {

    }

    public class B
    {
        public B()
        {
            int a = 1;
        }

        public void Index()
        {

        }
    }

   
}
