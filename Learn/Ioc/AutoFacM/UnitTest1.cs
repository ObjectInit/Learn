using System;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Learn.Ioc.AutoFacM
{
    [TestClass]
    public class UnitTest1
    {
        private static IContainer Container { get; set; }

        [TestMethod]
        public void TestMethod1()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ConsoleOutput>().As<IOutput>();
            builder.RegisterType<TodayWriter>().As<IDateWriter>();
            Container = builder.Build();

            // The WriteDate method is where we'll make use
            // of our dependency injection. We'll define that
            // in a bit.
            WriteDate();
        }
        public static void WriteDate()
        {
            // Create the scope, resolve your IDateWriter,
            // use it, then dispose of the scope.
            using (var scope = Container.BeginLifetimeScope())
            {
                var writer = scope.Resolve<IDateWriter>();
                var writer2 = scope.Resolve<IDateWriter>();
                writer.WriteDate();
            }
        }
    }

    public interface IOutput
    {
        void Write(string content);
    }

    //这个IOutput接口的实现 
    //实际上是我们写控制台的方式。从技术上讲 
    //我们也可以实现IOutput来写调试 
    //或跟踪……或其他地方。
    public class ConsoleOutput : IOutput
    {
        public void Write(string content)
        {
            Console.WriteLine(content);
        }
    }

    // This interface decouples the notion of writing
    // a date from the actual mechanism that performs
    // the writing. Like with IOutput, the process
    // is abstracted behind an interface.
    public interface IDateWriter
    {
        void WriteDate();
    }

    // This TodayWriter is where it all comes together.
    // Notice it takes a constructor parameter of type
    // IOutput - that lets the writer write to anywhere
    // based on the implementation. Further, it implements
    // WriteDate such that today's date is written out;
    // you could have one that writes in a different format
    // or a different date.
    public class TodayWriter : IDateWriter,IDisposable
    {
        private IOutput _output;
        public TodayWriter(IOutput output)
        {
            this._output = output;
        }

        public void WriteDate()
        {
            this._output.Write(DateTime.Today.ToShortDateString());
        }

        public void Dispose()
        {
            Console.WriteLine("执行释放");
        }
    }
}
