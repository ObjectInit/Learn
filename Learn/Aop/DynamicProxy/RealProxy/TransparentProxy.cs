#region  <<版本注释>>
/* ========================================================== 
// <copyright file="TransparentProxycs.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：TransparentProxycs 
* 创 建 者：Administrator 
* 创建时间：2019/5/17 14:32:39 
* =============================================================*/
#endregion

using System;

namespace Learn.UnitTest.Aop.DynamicProxy.RealProxy
{
    public static class TransparentProxy
    {
        public static T Create<T>()
        {
            T instance = Activator.CreateInstance<T>();
            MyRealProxy<T> realProxy = new MyRealProxy<T>(instance);
            T transparentProxy = (T)realProxy.GetTransparentProxy();
            return transparentProxy;
        }
    }
}
