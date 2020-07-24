#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcYearEnd.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：自动凭证结转
* 创 建 者：张莉
* 创建时间：2019/9/30 9:22:27
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 自动凭证结转
    /// 1 描述：定义自动凭证结转参数
    /// 2 约定：无
    /// 3 业务逻辑：
    ///     新增前：核算期校验
    ///     修改前：核算期校验
    ///     删除前：被定义科目使用或结转凭证使用，不能删除
    /// </summary>
    [ClassData("cname", "自动凭证结转", "vision", 1)]
    public class AcYearEnd : PubOthers
    {
        /// <summary>
        /// 字段定义、查询参数定义、子查询定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // * 字段定义
            // 启用预留字段
            this.AddField("period", ApiTask.L("核算期"), EFieldType.整数, string.Empty, "a9");
            this.AddField("vclass", ApiTask.L("预算方案"), EFieldType.关联, "AcVclass", "a8");
            this.AddField("vtype", ApiTask.L("凭证类型"), EFieldType.关联, "AcVoucherType", "a7");

            // 更改显示名称
            ((FieldDefine)ListField["ddesc"]).DisplayName = ApiTask.L("摘要");
        }

        #region 框架方法重写

        /// <summary>
        /// 自动凭证结转新增前处理的逻辑：
        /// 1、数据格式验证
        /// 2、存在性验证
        /// 3、唯一性验证
        /// 4、核算期校验
        /// </summary>
        /// <param name="data">自动凭证结转实体</param>
        protected override void OnInsertBefore(SData data)
        {
            if (data != null)
            {
                // 判断逻辑 **********
                // * 数据格式验证
                Valid(data);

                // * 存在性验证
                this.CheckRefExist(data, "vclass", "vtype");

                // * 唯一性验证
                if (!this.IsUnique(new SData("dcode", data["dcode"])))
                {
                    throw new Exception($"{((FieldDefine)ListField["dcode"]).DisplayName}{ApiTask.LAma(AcLang.Repeat)}");
                }

                // 业务逻辑 **********
                // * 核算期校验
                PeriodHelper.Check(int.Parse(data["period"].ToString()), ApiTask);

                base.OnInsertBefore(data);
            }
        }

        /// <summary>
        /// 自动凭证结转修改前处理的逻辑：
        /// 1、id赋值
        /// 2、数据格式验证
        /// 3、存在性验证
        /// 4、唯一性验证
        /// 5、核算期校验
        /// </summary>
        /// <param name="oldData">修改前数据</param>
        /// <param name="updateData">修改后数据</param>
        protected override void OnUpdateBefore(SData oldData, SData updateData)
        {
            // 数据准备 **********
            // * id赋值
            updateData["id"] = updateData["id"] ?? oldData["id"];

            // 判断逻辑 **********
            // * 数据格式验证
            Valid(updateData, false);

            // * 存在性验证
            this.CheckRefExist(updateData, "vclass", "vtype");

            // * 唯一性验证
            if (updateData.ContainsKey("dcode") && !updateData["dcode"].Equals(oldData["dcode"]) &&
                !this.IsUnique(new SData("id", updateData["id"], "dcode", updateData["dcode"])))
            {
                throw new Exception($"{((FieldDefine)ListField["dcode"]).DisplayName}{this.ApiTask.LAma(AcLang.Repeat)}");
            }

            // 业务逻辑 **********
            // * 核算期校验
            if (updateData.ContainsKey("period"))
            {
                PeriodHelper.Check(int.Parse(updateData["period"].ToString()), this.ApiTask);
            }

            base.OnUpdateBefore(oldData, updateData);
        }

        /// <summary>
        /// 删除前校验数据
        /// 1、被定义科目使用或结转凭证使用，不能删除
        /// </summary>
        /// <param name="data">对象</param>
        protected override void OnDeleteBefore(SData data)
        {
            // 业务逻辑 **********
            // * 被定义科目使用或结转凭证使用，不能删除
            this.CheckDeleteRef((int)data["id", 0]);

            base.OnDeleteBefore(data);
        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 验证数据格式：必填/长度/格式
        /// <param name="data">自动凭证结转实体</param>
        /// <param name="isInsert">true是新增,false是修改</param>
        /// </summary>
        private void Valid(SData data, bool isInsert = true)
        {
            this.CheckEmptyOrNull(data, isInsert, "dcode", "title", "period", "vclass", "vtype");

            this.CheckMaxLength("dcode", 80, data);

            this.CheckMaxLength("title", 150, data);

            this.CheckMaxLength("ddesc", 300, data);
        }
        #endregion

    }
}
