using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJ.FrontEnd.Base;

namespace Learn.Ac.Controllers
{
    public class LDemoFormListController : EzFormList
    {
        public LDemoFormListController() : this("选择勾对数据", "AcVoucherDSub")
        {
            ContainBizField("SubUnit").ContainBizField("vclass");
        }
        public LDemoFormListController(string pageTitle, string apiEntity) : base(pageTitle, apiEntity)
        {
        }
    }
}