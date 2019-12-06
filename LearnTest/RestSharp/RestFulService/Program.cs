#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2019/12/6 14:30:20 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Learn.Console.RestSharp.RestFulService
{
    public class Program : IMain
    {
        public void Main(string[] args)
        { 
            try
            {
                System.Console.Title = "Restful服务端测试";
                PersonInfoQueryServices service = new PersonInfoQueryServices();
                WebServiceHost _serviceHost = new WebServiceHost(service, new Uri("http://127.0.0.1:7788/"));
                //或者第二种方法：WebServiceHost _serviceHost = new WebServiceHost(typeof(PersonInfoQueryServices), new Uri("http://127.0.0.1:7788/"));
                _serviceHost.Open();
                System.Console.WriteLine("Web服务已开启...");
                System.Console.WriteLine("输入任意键关闭程序！");
                System.Console.ReadKey();
                _serviceHost.Close();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Web服务开启失败：{0}\r\n{1}", ex.Message, ex.StackTrace);
            }
        }
    }
}
