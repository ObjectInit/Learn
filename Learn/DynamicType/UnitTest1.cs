using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Learn.DynamicType
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            dynamic staticType = new StaticMemberDynamicWrapper(typeof(String));
            Console.WriteLine(staticType.Concat("A", "B"));

            staticType = new StaticMemberDynamicWrapper(typeof(StaticTestType));
            Console.WriteLine(staticType.Method(5));
            staticType.Field = DateTime.Now;
            Console.WriteLine(staticType.Field);
            staticType.Property = Guid.NewGuid();
            Console.WriteLine(staticType.Property);
        }

        private static class StaticTestType
        {
            public static String Method(Int32 x) { return x.ToString(); }
#pragma warning disable 649 // Field is never assigned to, and will always have its default value
            public static DateTime Field;
#pragma warning restore 649
            public static Guid Property { get; set; }
        }
    }
}
