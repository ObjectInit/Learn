#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Startup.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Startup 
* 创 建 者：Administrator 
* 创建时间：2019/6/20 16:11:39 
* =============================================================*/
#endregion

using System.Web.Http;
using Owin;

// ReSharper disable once InvalidXmlDocComment
/// <summary>
/// 选装nuget 包 Microsoft.AspNet.WebApi.Owin
/// </summary>
namespace Learn.WebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
            appBuilder.UseWebApi(config);
        }
    }
}
