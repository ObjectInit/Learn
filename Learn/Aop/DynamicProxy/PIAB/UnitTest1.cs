using System;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.PolicyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Learn.Aop.DynamicProxy.PIAB
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// 调用的代码
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            try
            {
                //使用Policy Injection Application block模块
                IConfigurationSource configurationSource = ConfigurationSourceFactory.Create();
                PolicyInjector policyInjector = new PolicyInjector(configurationSource);
                PolicyInjection.SetPolicyInjector(policyInjector);

                var oUserTest1 = new User() { Name = "test2222", PassWord = "yxj" };
                var oUserTest2 = new User() { Name = "test3333", PassWord = "yxj" };
                var oUser = UserOperation.GetInstance();
                oUser.Test(oUserTest1);
                oUser.Test2(oUserTest1, oUserTest2);
            }
            catch (Exception ex)
            {
                //throw;
            }
        }
    }
}
