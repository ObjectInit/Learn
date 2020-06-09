using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJ.FrontEnd.Base;

namespace Learn.Ac.Controllers
{
    public class AcBalancy2Controller: AcBalanceController
    {
        protected override void ProcessOnInit()
        {
            base.ProcessOnInit();

            RunContext.QueryParms = new List<FieldDefine>() { };
        }
    }
}