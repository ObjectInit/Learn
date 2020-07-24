#region << 版 本 注 释 >>
/* ==============================================================================
// <copyright file="AcAnalySet.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：账龄分析时间设置
* 创 建 人：胡智
* 创建日期：2019-09-10 16:07:00
* ==============================================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 账龄分析时间设置
    /// 1.描述：无
    /// 2.约定：无
    /// 3.业务逻辑：无
    /// </summary>
    [ClassData("cname", "账龄分析时间设置", "vision", 1)]
    public class AcAnalySet : PubOthers
    {
        /// <summary>
        /// 预留字段定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            // 字段定义 **********
            base.OnCustomDefine();

            // 账龄分析类型
            this.AddField("acanalytype", ApiTask.L("类型"), EFieldType.关联, "AcAnalyType", "a9");

            // 账龄分析日期类型
            this.AddField("begintype", ApiTask.L("日期类型"), EFieldType.关联, "AcAnalyDtType", "a8");

            // 从>=
            this.AddField("begins", ApiTask.L("从>="), EFieldType.整数, string.Empty, "a7");

            // 账龄分析日期类型
            this.AddField("endtype", ApiTask.L("日期类型"), EFieldType.关联, "AcAnalyDtType", "a6");

            // 到<
            this.AddField("ends", ApiTask.L("到<"), EFieldType.整数, string.Empty, "a5");

            // 账龄分析类别
            this.AddField("acanalygenre", ApiTask.L("类别"), EFieldType.关联, "AcAnalyGenre", "a4");

            ((FieldDefine)ListField["ddesc"]).DisplayName = ApiTask.L("备注");
        }

        #region 框架方法重写

        /// <summary>
        /// 新增前逻辑
        /// 1、基类数据验证
        /// 2、验证数据格式(必填、长度、格式)
        /// 3、验证数据存在性(类型是否存在、日期类型是否存在)
        /// </summary>
        /// <param name="data">需要新增的数据</param>
        protected override void OnInsertBefore(SData data)
        {
            // PubOthers中代码和名称校验提示要先抛出，不能将其顺序调至末尾
            base.OnInsertBefore(data);

            // 判断逻辑 **********
            // * 验证数据格式(必填、长度、格式)
            CheckData(data);

            // * 验证数据存在性(类型是否存在、日期类型是否存在)
            this.CheckRefExist(data, "acanalytype", "acanalygenre", "begintype", "endtype");
        }

        /// <summary>
        /// 修改前逻辑
        /// 1、基类数据验证
        /// 2、验证数据格式 (必填、长度、格式)
        /// 3、验证数据存在性(类型是否存在、日期类型是否存在)
        /// </summary>
        /// <param name="oldData">修改前的数据</param>
        /// <param name="updateData">需要修改的数据</param>
        protected override void OnUpdateBefore(SData oldData, SData updateData)
        {
            // PubOthers中代码和名称校验提示要先抛出，不能将其顺序调至末尾
            base.OnUpdateBefore(oldData, updateData);

            // 判断逻辑 **********
            // * 验证数据格式(必填、长度、格式)
            CheckData(updateData, false);

            // * 验证数据存在性(类型是否存在、日期类型是否存在)
            this.CheckRefExist(updateData, "acanalytype", "acanalygenre", "begintype", "endtype");
        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 验证数据必填、长度、格式
        /// </summary>
        /// <param name="data">需要验证的数据</param>
        /// <param name="isInsert">是否新增</param>
        protected void CheckData(SData data, bool isInsert = true)
        {
            // 验证必填
            this.CheckEmptyOrNull(data, isInsert, "acanalytype", "acanalygenre", "begintype", "begins", "endtype", "ends");

            // 验证长度
            this.CheckMaxLength("dcode", 80, data);
            this.CheckMaxLength("title", 150, data);
            this.CheckMaxLength("ddesc", 300, data);

            // 验证格式(从>=)
            this.CheckFormat("begins", Regx.Integer, data, ApiTask.L("从>=") + ApiTask.LEnd("为大于等于0的整数"));

            // 验证格式(到<)
            this.CheckFormat("ends", Regx.Integer, data, ApiTask.L("到<") + ApiTask.LEnd("为大于等于0的整数"));
        }

        #endregion
    }
}
