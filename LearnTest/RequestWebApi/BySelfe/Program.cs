#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2019/7/12 15:18:42 
* =============================================================*/
#endregion

using System;
using Microsoft.Owin.Hosting;

namespace Learn.Console.RequestWebApi.BySelfe
{
    public class Program:IMain
    {
        public void Main(string[] args)
        {
            RunApp();
            //Post("http://localhost:62773/api/Person/CheckUserName", "userName=lwh");
            Post("http://localhost:8183/api/Person/CheckUserName2/123/321", "");
            System.Console.ReadLine();
        }

        private void RunApp()
        {
            var url = "http://localhost:8183/";
            var startOpts = new StartOptions(url)
            {

            };

            WebApp.Start<WebApi.Startup>(startOpts);
        }
        public static string Get(string url)
        {
            try
            {
                var request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                if (request != null)
                {
                    string retval = null;
                    init_Request(ref request);
                    using (var Response = request.GetResponse())
                    {
                        using (var reader = new System.IO.StreamReader(Response.GetResponseStream(), System.Text.Encoding.UTF8))
                        {
                            retval = reader.ReadToEnd();
                        }
                    }
                    return retval;
                }
            }
            catch
            {

            }
            return null;
        }
        public static string Post(string url, string data)
        {
            try
            {
                var request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                if (request != null)
                {
                    string retval = null;
                    init_Request(ref request);
                    request.Method = "POST";
                    request.ServicePoint.Expect100Continue = false;
                    request.ContentType = "application/json; charset=utf-8";
                    var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(data);
                    request.ContentLength = bytes.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    using (var response = request.GetResponse())
                    {
                        using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                        {
                            url = reader.ReadToEnd();
                        }
                    }
                    return retval;
                }
            }
            catch(Exception e)
            {

            }
            return null;
        }


        private static void init_Request(ref System.Net.HttpWebRequest request)
        {
            request.Accept = "text/json,*/*;q=0.5";
            request.Headers.Add("Accept-Charset", "utf-8;q=0.7,*;q=0.7");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, x-gzip, identity; q=0.9");
            request.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
            request.Timeout = 8000;
        } 
    }
}
