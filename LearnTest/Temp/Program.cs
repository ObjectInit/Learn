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

namespace Learn.Console.Temp
{
    public class Program : IMain
    {
        public void Main(string[] args)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var typeName = $"Learn.Console.Temp.A";//根据约定code就是实体名称
            object[] parameters = new object[1];
            parameters[0] = assembly.CreateInstance(typeName);

            var t=typeof(A);
            var method= t.GetMethod("Index");
            method.Invoke(parameters[0],null);
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
