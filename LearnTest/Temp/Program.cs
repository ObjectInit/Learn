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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using AutoMapper;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Learn.Console.Temp
{
    public class Person
    {
        public int Age { get; set; }

        public string Name { get; set; }
    }


    public class B
    {
        public string Name { get; set; }

        public virtual void Say() { }

        public B()
        {
            System.Console.WriteLine("Constrol.B");
        }
    }

    public class A : B
    {
        public A()
        {
            System.Console.WriteLine("Constrol.A");
        }
    }
    public class Program : IMain
    {
        public void Main(string[] args)
        {
            List<int> a = new List<int> { 1, 2, 3 };
            var currentIndex = a.IndexOf(2);
            var t= a.Take(currentIndex+1);
            var t2 = a.Skip(currentIndex);
            var index1 = a.IndexOf(1);
            var t3 = a.Take(index1+1);
            var t4 = a.Skip(index1);
            System.Console.ReadLine();
        }
    }


}
