using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Learn.Event
{
    #region 委托的定义
    /// <summary>
    /// 1.定义事件的公开类型传递附加信息
    /// </summary>
    public class NewMailEventArgs : EventArgs
    {
        private string _From;
        private string _To;
        private string _Object;

        public NewMailEventArgs(string @from, string to, string o)
        {
            _From = @from;
            _To = to;
            _Object = o;
        }

        public string From
        {
            get { return _From; }
            set { _From = value; }
        }

        public string To
        {
            get { return _To; }
            set { _To = value; }
        }

        public string Object
        {
            get { return _Object; }
            set { _Object = value; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class NewMailManager
    {
        /// <summary>
        ///  2.定义事件成员
        /// </summary>
        public event EventHandler<NewMailEventArgs> newMail;

        /// <summary>
        /// 3.定义负责引发事件的方法来通知已登记的对象。
        /// </summary>
        protected virtual void OnNewMail(NewMailEventArgs e)
        {
            //线程安全  将委托引用复制到一个临时变量中
            EventHandler<NewMailEventArgs> temp = Volatile.Read(ref newMail);
            if (temp != null) temp(this, e);
        }
    }
    #endregion

    #region 编译器如何实现理解事件

    public class NewMailManagerIl
    {
        /// <summary>
        /// 初始化一个null的委托字段
        /// </summary>
        public EventHandler<NewMailEventArgs> newMail = null;

        /// <summary>
        /// 以线程安全的方式向事件添加委托
        /// </summary>
        public void Add_NewMail(EventHandler<NewMailEventArgs> value)
        {
            EventHandler<NewMailEventArgs> preHandler;
            EventHandler<NewMailEventArgs> newMail = this.newMail;
            do
            {
                preHandler = newMail;
                EventHandler<NewMailEventArgs> newHandler =
                    (EventHandler<NewMailEventArgs>)Delegate.Combine(preHandler, value);
                newMail = Interlocked.CompareExchange<EventHandler<NewMailEventArgs>>(ref this.newMail, newHandler,
                    preHandler);
            } while (newMail != preHandler);
        }

        /// <summary>
        /// 以线程安全的方式向事件移除委托
        /// </summary>
        /// <param name="value"></param>
        public void Remove_NewMail(EventHandler<NewMailEventArgs> value)
        {
            EventHandler<NewMailEventArgs> preHandler;
            EventHandler<NewMailEventArgs> newMail = this.newMail;
            do
            {
                preHandler = newMail;
                EventHandler<NewMailEventArgs> newHandler =
                    (EventHandler<NewMailEventArgs>)Delegate.Remove(preHandler, value);
                newMail = Interlocked.CompareExchange<EventHandler<NewMailEventArgs>>(ref this.newMail, newHandler,
                    preHandler);
            } while (newMail != preHandler);
        }
    }

    #endregion

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

        }
    }
}
