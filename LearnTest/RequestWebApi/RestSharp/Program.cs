#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2019/7/12 15:29:57 
* =============================================================*/
#endregion

using Microsoft.Owin.Hosting;

namespace Learn.Console.RequestWebApi.RestSharp
{
    public class Program:IMain
    {
        public void Main(string[] args)
        {
            RunApp();
        }

        private void RunApp()
        {
            var url = "http://localhost:8183/";
            var startOpts = new StartOptions(url)
            {

            };

            WebApp.Start<WebApi.Startup>(startOpts);
        }
    }
}
