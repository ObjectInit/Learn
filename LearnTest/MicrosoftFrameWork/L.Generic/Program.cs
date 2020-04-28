#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2020/4/15 12:19:28 
* =============================================================*/
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Console.MicrosoftFrameWork.L.Generic
{
    public class Program : IMain
    {
        public void Main(string[] args)
        {
            ValueTypePerfTest();
        }


        private static void ValueTypePerfTest()
        {
            const Int32 count = 100000000;

            using (new OperationTimer("List<Int32>"))
            {
                List<Int32> l = new List<int>();
                for (var n = 0; n < count; n++)
                {
                    l.Add(n);       // 不装箱
                    Int32 x = l[n]; //不拆箱
                }

                l = null; //确保进行垃圾回收
            }

            using (new OperationTimer("ArrayList of Int32"))
            {
                ArrayList l = new ArrayList();
                for (var n = 0; n < count; n++)
                {
                    l.Add(n);       //  装箱
                    Int32 x = (Int32)l[n]; //拆箱
                }
                l = null; //确保进行垃圾回收
            }

            System.Console.ReadLine();

        }
    }

    public sealed class OperationTimer : IDisposable
    {
        private Stopwatch m_stopwatch;
        private string m_text;
        private Int32 m_collectionCount;

        public OperationTimer(string text)
        {
            PrepareForOperation();
            m_text = text;
            m_collectionCount = GC.CollectionCount(0);
            m_stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            System.Console.WriteLine("{0} (GCs={1,3}) {2}", m_stopwatch.Elapsed, GC.CollectionCount(0) - m_collectionCount, m_text);
        }

        private static void PrepareForOperation()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
