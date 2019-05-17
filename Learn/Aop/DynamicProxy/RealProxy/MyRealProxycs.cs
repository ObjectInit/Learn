#region  <<版本注释>>
/* ========================================================== 
// <copyright file="MyRealProxycs.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：MyRealProxycs 
* 创 建 者：Administrator 
* 创建时间：2019/5/17 14:06:30 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Aop.DynamicProxy.RealProxy
{
    public class MyRealProxy<T> :System.Runtime.Remoting.Proxies.RealProxy
    {
        private T tTarget;

        public MyRealProxy(T target)
        :base(typeof(T))
        {
            this.tTarget = target;
        }
        public override IMessage Invoke(IMessage msg)
        {
            PreProceede(msg);
            IMethodCallMessage callMessage = (IMethodCallMessage)msg;
            object returnValue = callMessage.MethodBase.Invoke(this.tTarget, callMessage.Args); 
            PostProceede(msg);
            return new ReturnMessage(returnValue, new object[0], 0, null, callMessage);
        }

        public void PreProceede(IMessage msg)
        {
            Console.WriteLine("检查软件版本信息");
        }
        public void PostProceede(IMessage msg)
        {
            Console.WriteLine("保存本地登录缓存");
        }
    }
}
