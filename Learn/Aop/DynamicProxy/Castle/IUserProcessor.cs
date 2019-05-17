using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Aop.DynamicProxy.Castle
{
    /// <summary>
    /// 定义一个接口规范
    /// </summary>
    public interface IUserProcessor
    {
        void RegUser(User user);
    }
}
