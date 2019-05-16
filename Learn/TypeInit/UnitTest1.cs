using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Learn.TypeInit
{

    internal class Foo
    {
        public Foo() { Console.WriteLine("Foo 对象构造函数"); }
        public static string Field = GetString("初始化 Foo 静态成员变量!");

        public static string GetString(string s)
        {
            Console.WriteLine(s);
            return s;
        }
    } 
    internal class FooStatic
    {
        static FooStatic() { Console.WriteLine("FooStatic 类构造函数"); }
        FooStatic() { Console.WriteLine("FooStatic 对象构造函数"); }

        public static string Field = GetString("初始化 FooStatic 静态成员变量!");
        public static string GetString(string s)
        {
            Console.WriteLine(s);
            return s;
        }
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Console.WriteLine("Main 开始 ...");
            // Foo foo = new Foo();
            // Foo.GetString("手动调用 Foo.GetString() 方法!");
            string info = Foo.Field;

            //var t=FooStatic.Field;
            FooStatic.GetString("手动调用 FooStatic.GetString() 方法!");
            //string infoStatic = FooStatic.Field;

            Console.ReadLine();
        }
    }
}
