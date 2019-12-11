#region  <<版本注释>>
/* ========================================================== 
// <copyright file="StringExtendcs.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：StringExtendcs 
* 创 建 者：Administrator 
* 创建时间：2019/12/11 11:16:59 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Learn.Console.Encoding
{
    /// <summary>
    /// 字符串扩展
    /// </summary>
    public static class StringExtend
    {
        /// <summary>
        /// 过滤掉字符串中的控制字符
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns></returns>
        public static string StripControlChars(this string input)
        {
            return Regex.Replace(input, @"[^\x20-\x7F]", "");
        }
    }
}
