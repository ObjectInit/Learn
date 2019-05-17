using System; 
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Learn.Aop.DynamicProxy.RealProxy
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            User user = new User() { Name = "Eleven", Password = "123123123123" };
            UserProcessor userprocessor = TransparentProxy.Create<UserProcessor>();
            userprocessor.RegUser(user);
        }
    }
}
