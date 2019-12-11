#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2019/12/11 11:19:31 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Console.Encoding
{
    public class Program : IMain
    {
        public void Main(string[] args)
        {
            // 经过ASCII编码 发现 151780.78  是带有\u202c 的 需要去除 调用下面方法去除
            string s = "151780.78‬";
            System.Console.WriteLine(EnCodingHelper.String2Unicode(s));
            s = s.StripControlChars();
            System.Console.WriteLine(EnCodingHelper.String2Unicode(s));
            System.Console.ReadLine();
        }
    }
}
