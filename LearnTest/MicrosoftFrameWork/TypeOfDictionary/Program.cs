#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2019/7/2 11:46:36 
* =============================================================*/
#endregion

using System.Collections;
using System.Collections.Generic;

namespace Learn.Console.MicrosoftFrameWork.TypeOfDictionary
{
    public class Program : IMain
    {
        //定义
        readonly Dictionary<string, string> openWith = new Dictionary<string, string>();

        public Program()
        {
            //添加元素
            openWith.Add("txt", "notepad.exe");
            openWith.Add("bmp", "paint.exe");
            openWith.Add("dib", "paint.exe");
            openWith.Add("rtf", "wordpad.exe");
        }

        public void Main(string[] args)
        {
            // 遍历value, Second Method
            Dictionary<string, string>.ValueCollection valueColl = openWith.Values;
            foreach (string s in valueColl)
            {
                System.Console.WriteLine("Second Method, Value = {0}", s);
            }
            //遍历
            IDictionary idic = openWith;
            foreach (var item in idic)
            {
                var e = (DictionaryEntry) item;
            }

        }
    }
}
