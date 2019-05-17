using System;
using Castle.DynamicProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Learn.Aop.DynamicProxy.Castle
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            User user = new User() { Name = "Eleven", Password = "123123123123" };
            ProxyGenerator generator = new ProxyGenerator(); //创建一个代理生产器
            MyInterceptor interceptor = new MyInterceptor(); //创建一个拦截器
            var userprocessor = generator.CreateClassProxy<UserProcessor>(interceptor);//创建一个基于拦截器的代理对象
            userprocessor.RegUser2(user);
        }
    }
}
