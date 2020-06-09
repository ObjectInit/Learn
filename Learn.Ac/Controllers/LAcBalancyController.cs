using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJ.FrontEnd.Base;

namespace Learn.Ac.Controllers
{
    public class LAcBalancyController:WebFormList
    {
        protected override void ProcessOnInit()
        {
            RunContext = new ContextFormList();
            //页面title赋值
            RunContext.Title = "title";

            RunContext.ApiEntity = "AcBalance";

            RunContext.QueryParms = new List<FieldDefine>();

            RunContext.Fields = new List<FieldDefine>()
            {
                new FieldDefine()
                {
                    FieldName = "account",
                    DisplayName = "科目",
                    FieldType = SJ.Global.EFieldType.字符串
                },
                new FieldDefine()
                {
                    FieldName = "sh.a_sum_amountd",
                    DisplayName = "借方金额",
                    FieldType = SJ.Global.EFieldType.数值
                },
                new FieldDefine()
                {
                    FieldName = "sh.a_sum_amountc",
                    DisplayName = "贷方金额",
                    FieldType = SJ.Global.EFieldType.数值
                }
            };
            RunContext.UserButtom = new List<ButtomDefine>();
        }
    }
}