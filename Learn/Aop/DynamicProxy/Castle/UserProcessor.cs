#region  <<版本注释>>
/* ========================================================== 
// <copyright file="UserProcessor.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：UserProcessor 
* 创 建 者：Administrator 
* 创建时间：2019/5/17 11:25:45 
* =============================================================*/
#endregion

using System;

namespace Learn.UnitTest.Aop.DynamicProxy.Castle
{
    /// <summary>
    /// 此类型会被代理
    /// </summary>
    public class UserProcessor : IUserProcessor
    {
        /// <summary>
        /// 实现必须是虚拟的
        /// </summary>
        /// <param name="user"></param>
        public virtual void RegUser(User user)
        {
            Console.WriteLine("用户登录。Name:{0},PassWord:{1}", user.Name, user.Password);
        }

        /// <summary>
        /// 实现必须是虚拟的
        /// </summary>
        /// <param name="user"></param>
        public virtual void RegUser2(User user)
        {
            Console.WriteLine("用户登录。Name:{0},PassWord:{1}", user.Name, user.Password);
        }
    }
}
