using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJ.FrontEnd.Base;
using SJ.Global;

namespace Learn.Ac.Controllers
{
    public class PersonController : WebFormList
    {
        protected override void ProcessOnInit()
        {
            RunContext = new ContextFormList()
            {
                ApiEntity = "person",
            };

            RunContext.QueryParms = new List<FieldDefine>();

            RunContext.QueryParms.AddRange(new List<FieldDefine>()
            {
                new FieldDefine
                {
                    FieldName = "dcode",
                    DisplayName = "代码",
                    FieldType = EFieldType.字符串,
                    Width = 10 // 核算期对应业务日期的宽度
                },
                new FieldDefine()
                {
                    FieldName = "acaccount",
                    DisplayName = "科目",
                    FieldType = EFieldType.关联,
                    PopUrl =$"Pop?Biz=AcAccount&dcode=!z*",
                    Width = 15
                }
            });

            RunContext.Fields = new List<FieldDefine>();

            RunContext.Fields.AddRange(new List<FieldDefine>()
            {
                new FieldDefine()
                {
                    FieldName = "dcode",
                    DisplayName = "代码",
                    FieldType =EFieldType.字符串,
                    Width = 10
                },
                new FieldDefine()
                {
                    FieldName = "title",
                    DisplayName = "名称",
                    FieldType =EFieldType.字符串,
                    Width = 10
                },
                new FieldDefine()
                {
                    FieldName = "desca",
                    DisplayName = "描述",
                    FieldType =EFieldType.字符串,
                    Width = 10
                },
                new FieldDefine()
                {
                    FieldName = "acaccount",
                    DisplayName = "科目",
                    FieldType =EFieldType.关联,
                    PopUrl =$"Pop?Biz=AcAccount&dcode=!z*",
                    Width = 10
                }
            });

            RunContext.UserButtom = new List<ButtomDefine>();
        }
    }
}