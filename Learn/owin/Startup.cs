#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Startup.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Startup 
* 创 建 者：Administrator 
* 创建时间：2019/6/20 14:02:18 
* =============================================================*/
#endregion

using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

namespace Learn.UnitTest.owin
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.Run(HandleRequest);
        }

        static Task HandleRequest(IOwinContext context)
        {
            context.Response.ContentType = "text/plain";
            return context.Response.WriteAsync("Hello, world!");
        }
    }
}
