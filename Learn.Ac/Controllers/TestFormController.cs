using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJ.FrontEnd.Base;

namespace Learn.Ac.Controllers
{
    public class TestFormController : WebFormList
    {
        protected override void ProcessOnInit()
        {
            RunContext = new ContextFormList() { };
            //页面title赋值
            RunContext.Title = "title";

            RunContext.ApiEntity = "AcRecoD";

            RunContext.QueryParms = new List<FieldDefine>();

            RunContext.Fields = new List<FieldDefine>()
            {
                new FieldDefine()
                {
                    FieldName = "acvoucherd.amountd",
                    DisplayName = "借方金额",
                    FieldType = SJ.Global.EFieldType.数值
                }
            };

            RunContext.UserButtom = new List<ButtomDefine>();
        }
    }
}