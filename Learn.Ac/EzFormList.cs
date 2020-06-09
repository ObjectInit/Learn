using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJ.FrontEnd.Base;
using SJ.Global;
using SJ.WebApi.Client;

namespace Learn.Ac
{
    public abstract  class EzFormList : WebFormList
    {
        /// <summary>
        /// biz 定义
        /// </summary>
        private SData m_bizDefine;

        private SData m_bizFields;

        private List<FieldDefine> m_queryFieldNames;

        /// <summary>
        /// 一般单实体的约定 可以按照实体定义来给定字段显示
        /// </summary>
        /// <param name="pageTitle"></param>
        /// <param name="apiEntity"></param>
        protected EzFormList(string pageTitle, string apiEntity = "")
        {
            RunContext = new ContextFormList() { };

            // 校验-为业务
            if (string.IsNullOrEmpty(pageTitle))
            {
                throw new Exception("页面title不能为空");
            }

            if (string.IsNullOrEmpty(apiEntity))
            {
                throw new Exception("暂时未实现");
            }

            m_bizDefine = WebApiClient.BizDefine(apiEntity);

            if (m_bizDefine != null)
            {
                m_bizFields = m_bizDefine["fields"] as SData;
            }
            //页面title赋值
            RunContext.Title = pageTitle;
        }

        protected override void ProcessOnInit()
        {
            RunContext.QueryParms = new List<FieldDefine>();

            RunContext.Fields = new List<FieldDefine>();

            RunContext.UserButtom = new List<ButtomDefine>();
        }

        protected override void GetDataAfter()
        {
            RunContext.Response.ClientAddScript("window.top.layer.close(window.top.layer.index)");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="disPlayName"></param>
        /// <returns></returns>
        public EzFormList ContainBizField(string name,string disPlayName="")
        {
            
            return this;
        }

        public EzFormList ContainAgreedField(string name,string disPlayName)
        {
            return this;
        }
    }
}