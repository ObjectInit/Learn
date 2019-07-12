using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Learn.UnitTest.MicrosoftFrameWork
{
    [TestClass]
    public class SourceDis
    {
        [TestMethod]
        public void TestMethod1()
        {
            A a = new A();
        }
    }


    public class A:IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

          ~A()
        {

        }
    }
}
