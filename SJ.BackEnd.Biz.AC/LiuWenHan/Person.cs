#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Person.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Person
* 创 建 者：Administrator
* 创建时间：2020/7/24 15:39:44
* =============================================================*/
#endregion

using System;
using SJ.BackEnd.Base;
using SJ.Global;

namespace SJ.BackEnd.Biz.AC
{
    [ClassData("cname", "人员", "vision", 1)]
    public class Person : BizTableCodeOthers
    {
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            this.AddField("desca", "描述", EFieldType.字符串, RealField: "s7");
            this.AddField("acaccount", "科目", EFieldType.关联, RefBiz: "acaccount", RealField: "a9");
            this.AddSubQuery("acvoucherd", "凭证", "AcVoucherD", "id=id");
        }

    }
}
