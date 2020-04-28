#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program_Node.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program_Node 
* 创 建 者：Administrator 
* 创建时间：2020/4/16 9:18:02 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Console.MicrosoftFrameWork.L.Generic
{
    /// <summary>
    /// 泛型-表链接
    /// </summary>
    public class Program_Node : IMain
    {
        public void Main(string[] args)
        {
            Node<char> head = new Node<char>('C');
            head = new Node<char>('B', head);
            head = new Node<char>('A',head);

            System.Console.WriteLine(head.ToString());

            System.Console.ReadLine();
        }
    }

    public class Node<T>
    {
        private T m_data;
        private Node<T> m_next;

        public Node(T data, Node<T> next)
        {
            this.m_data = data;
            this.m_next = next;
        }

        public Node(T data) : this(data, null)
        {

        }

        public override string ToString()
        {
            return $"{this.m_data}{(this.m_next != null ? this.m_next.ToString() : string.Empty)}";
        }
    }



}
