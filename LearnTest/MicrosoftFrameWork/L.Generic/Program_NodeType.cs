#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program_NodeType.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program_NodeType 
* 创 建 者：Administrator 
* 创建时间：2020/4/23 14:00:34 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Console.MicrosoftFrameWork.L.Generic
{
    public class Node
    {
        protected Node m_next;

        public Node(Node next)
        {
            this.m_next = next;
        }
    }

    public class TypeNode<T> : Node
    {
        public T m_data;

        public TypeNode(T data) : this(data, null)
        {

        }

        public TypeNode(T data,Node next):base(next)
        {
            this.m_data = data;
        }

        public override string ToString()
        {
            return this.m_data.ToString() + (m_next == null ? string.Empty : m_next.ToString());
        }
    }
    class Program_NodeType:IMain
    {
        public void Main(string[] args)
        {
        }
    }
}
