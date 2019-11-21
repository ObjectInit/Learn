#region  <<版本注释>>
/* ========================================================== 
// <copyright file="A.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：A 
* 创 建 者：Administrator 
* 创建时间：2019/11/21 15:41:58 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Console.AutoMapper.Simple
{
    public class A
    {
        public static implicit operator B(A a)
        {
            return new B();
        }

        public static implicit operator A(B a)
        {
            return new A();
        }
    }

}
