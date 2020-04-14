#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2020/3/31 13:52:41 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Learn.Console.L.Driver
{
    class Program : IMain
    {
        public void Main(string[] args)
        {
            IWebDriver web = new ChromeDriver(@"C:\Users\Administrator\Downloads\chromedriver_win32 (2)");

            web.Navigate().GoToUrl("http://localhost:56668/");

            var element = web.FindElement(By.CssSelector("[postevent=login]"));
            element.Click();
            
        }
    }
}
