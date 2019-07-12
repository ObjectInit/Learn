using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Learn.UnitTest.DesignPattern
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            dynamic a = new { };
            a.Name = "刘文汉";
        }
    }
}
