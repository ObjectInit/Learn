using System;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once InvalidXmlDocComment
/// <summary>
/// owin简单使用
/// </summary>
namespace Learn.owin
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var url = "https://localhost:8180/";
            var startOpts = new StartOptions(url)
            {

            };
            using (WebApp.Start<Startup>(startOpts))
            {
                Console.WriteLine("Server run at " + url + " , press Enter to exit.");
                Console.ReadLine();
            }
        }
    }
}
