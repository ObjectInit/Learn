#region  <<版本注释>>
/* ========================================================== 
// <copyright file="MyInterceptor.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：MyInterceptor 
* 创 建 者：Administrator 
* 创建时间：2019/5/17 11:18:52 
* =============================================================*/
#endregion

using System;
using Castle.DynamicProxy;

namespace Learn.UnitTest.Aop.DynamicProxy.Castle
{
    /// <summary>
    /// 创建一个自定义拦截器
    /// </summary>
    public class MyInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            PreProceed(invocation);//调用前
            invocation.Proceed();
            PostProceed(invocation);//调用后
        }

        /// <summary>
        /// 执行前
        /// </summary>
        /// <param name="invocation"></param>
        public void PreProceed(IInvocation invocation)
        {
            Console.WriteLine("检查软件版本信息");
        }

        /// <summary>
        /// 执行后
        /// </summary>
        /// <param name="invocation"></param>
        public void PostProceed(IInvocation invocation)
        {
            Console.WriteLine("保存本地登录缓存");
        }

    }


}
