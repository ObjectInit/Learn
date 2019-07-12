#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Test.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Test 
* 创 建 者：Administrator 
* 创建时间：2019/6/20 16:18:44 
* =============================================================*/
#endregion

using Microsoft.Owin.Hosting;

namespace Learn.Console.Owin
{
    public class Program:IMain
    {
        public void Main(string[] args)
        {
            var url = "http://localhost:8182/";
            var startOpts = new StartOptions(url)
            {

            };
            using (WebApp.Start<Startup>(startOpts))
            {
                System.Console.WriteLine("Server run at " + url + " , press Enter to exit.");
                System.Console.ReadLine();
            }
        }
    }
}
