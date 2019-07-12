using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Learn.UnitTest.Aop.Static
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Order order = new Order() { Id = 1, Name = "lee", Count = 10, Price = 100.00, Desc = "订单测试" };
            IOrderProcessor orderProcess = new OrderProcessorDecorator(new OrderProcessor()); //通过自己的实现达到注入到系统流程中
            orderProcess.Submit(order);
        }
    }
}
