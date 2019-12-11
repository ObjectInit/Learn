#region  <<版本注释>>
/* ========================================================== 
// <copyright file="EnCodingHelper.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：EnCodingHelper 
* 创 建 者：Administrator 
* 创建时间：2019/12/11 11:24:00 
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
    public class EnCodingHelper
    {
        /// <summary>
        /// 含中文字符串转ASCII
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Str2ASCII(String str)
        {
            //这里我们将采用2字节一个汉字的方法来取出汉字的16进制码
            byte[] textbuf = System.Text.Encoding.Default.GetBytes(str);
            //用来存储转换过后的ASCII码
            string textAscii = string.Empty;

            foreach (var t in textbuf)
            {
                textAscii += t.ToString("X");
            }
            return textAscii;

        }

        /// <summary>
        /// ASCII转含中文字符串
        /// </summary>
        /// <param name="textAscii">ASCII字符串</param>
        /// <returns></returns>
        public static string ASCII2Str(string textAscii)
        {
            int k = 0;//字节移动偏移量

            byte[] buffer = new byte[textAscii.Length / 2];//存储变量的字节

            for (int i = 0; i < textAscii.Length / 2; i++)
            {
                //每两位合并成为一个字节
                buffer[i] = byte.Parse(textAscii.Substring(k, 2), System.Globalization.NumberStyles.HexNumber);
                k = k + 2;
            }
            //将字节转化成汉字
            return System.Text.Encoding.Default.GetString(buffer);
        }

        /// <summary>
        /// 字符串转Unicode
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>Unicode编码后的字符串</returns>
        internal static string String2Unicode(string source)
        {
            var bytes =System.Text.Encoding.Unicode.GetBytes(source);
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i += 2)
            {
                stringBuilder.AppendFormat("\\u{0}{1}", bytes[i + 1].ToString("x").PadLeft(2, '0'), bytes[i].ToString("x").PadLeft(2, '0'));
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Unicode转字符串
        /// </summary>
        /// <param name="source">经过Unicode编码的字符串</param>
        /// <returns>正常字符串</returns>
        internal static string Unicode2String(string source)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(source, x => Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)).ToString());
        }
    }
}
