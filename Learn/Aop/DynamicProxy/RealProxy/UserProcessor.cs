#region  <<版本注释>>
/* ========================================================== 
// <copyright file="UserProcessor.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：UserProcessor 
* 创 建 者：Administrator 
* 创建时间：2019/5/17 14:39:41 
* =============================================================*/
#endregion 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Aop.DynamicProxy.RealProxy
{
    public class UserProcessor: MarshalByRefObject,IUserProcessor
    {
        public void RegUser(User user)
        {
            Console.WriteLine("用户登录。用户名称{0} Password{1}", user.Name, user.Password);
        }
    }
}
