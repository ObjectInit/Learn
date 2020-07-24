#region  <<版本注释>>
/* ========================================================== 
// <copyright file="AcBank.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcBank 
* 创 建 者：张莉 
* 创建时间：2020/4/30 10:01:10 
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 银行账户
    /// 1 描述：定义银行的账户信息
    /// 2 约定：无
    /// 3 业务逻辑：无
    /// </summary>
    [ClassData("cname", "银行账户", "vision", 1, "tcode", 1)]
    public class AcBank : PubOthers
    {
        /// <summary>
        /// 字段定义、查询参数定义、子查询定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // * 字段定义
            // 启用预留字段
            this.AddField("contact", ApiTask.L("联系人"), EFieldType.字符串, string.Empty, "s0");
            this.AddField("phone", ApiTask.L("电话"), EFieldType.字符串, string.Empty, "s1");
            this.AddField("fax", ApiTask.L("传真"), EFieldType.字符串, string.Empty, "s2");
            this.AddField("address", ApiTask.L("地址"), EFieldType.字符串, string.Empty, "s3");
        }

        #region 框架方法重写

        /// <summary>
        /// 重写长度验证
        /// </summary>
        /// <param name="data">银行账户实体</param>
        protected override void CheckLength(SData data)
        {
            this.CheckMaxLength("dcode", 80, data);
            this.CheckMaxLength("title", 150, data);
            this.CheckMaxLength("contact", 100, data);
            this.CheckMaxLength("phone", 100, data);
            this.CheckMaxLength("fax", 100, data);
            this.CheckMaxLength("address", 100, data);
            this.CheckMaxLength("ddesc", 300, data);
        }
        #endregion
    }
}
