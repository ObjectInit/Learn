#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Singleton.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Singleton 
* 创 建 者：Administrator 
* 创建时间：2019/5/24 9:48:40 
* =============================================================*/
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Learn.UnitTest.DesignPattern
{
    /// <summary>
    /// 单例模式
    /// </summary>
    public class Singleton
    {
        private static Singleton uniqueInstance;

        /// <summary>
        /// 定义一个标识确保线程同步
        /// </summary>
        private static readonly object locker = new object(); 
        /// <summary>
        /// 确保不能构造实例
        /// </summary>
        private Singleton()
        {

        }

        /// <summary>
        /// 全局访问点(单线程下没问题  多线程就会创建多个 singleton 实例)
        /// </summary>
        /// <returns></returns>
        public static Singleton GetSingleThreadInstance()
        {
            if (uniqueInstance == null) uniqueInstance = new Singleton();
            return uniqueInstance;
        }

        public static Singleton GetMultithreadingInstance()
        {
            if (uniqueInstance == null)
            {
                lock (locker)
                {
                    if (uniqueInstance == null)
                    {
                        uniqueInstance = new Singleton();
                    }
                }
            }

            return uniqueInstance;
        }
    }

    /// <summary>
    /// 单例模式讲解
    /// </summary>
    [TestClass]
    public class SingletonTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            dynamic a = new { };
            a.Name = "刘文汉";
        }
    }
}
