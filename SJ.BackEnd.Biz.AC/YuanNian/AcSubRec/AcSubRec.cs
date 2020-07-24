#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcSubRec.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcSubRec 科目对账实体
* 创 建 者：袁炼
* 创建时间：2019/9/05 9:39:29
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 科目对账
    /// 1 描述：无
    /// 2 约定：
    ///     1.科目对账数据存在科目表中
    ///     2.自定义字段中根据ApiTask.Tcode()进行下设tcode定义
    /// 3 业务逻辑：无
    /// </summary>
    [ClassData("cname", "科目对账", "vision", 1, "tcode", 1)]
    public class AcSubRec : BizTableCode
    {
        public override sealed string TableName
        {
            get => "account";
            set { TableName = "account"; }
        }

        /// <summary>
        /// 自定义字段
        /// </summary>
        protected override void OnCustomDefine()
        {
            // 字段定义 **********
            // 启用预留字段
            // 获取下设tcode
            var data = ApiTask.Tcode();
            foreach (var item in data)
            {
                if (!item.Key.Contains(AcConst.RecoTcode))
                {
                    this.AddField(item.Key, "下设" + item.Value.ToString(), EFieldType.开关, string.Empty, string.Empty);
                }
            }

            // 更改显示名称
            ((FieldDefine)ListField["dcode"]).DisplayName = this.ApiTask.L("代码");
            ((FieldDefine)ListField["title"]).DisplayName = this.ApiTask.L("名称");
            ((FieldDefine)ListField["ddesc"]).DisplayName = this.ApiTask.L("备注");

            this.AddSubQuery("acreconref", "科目对账与科目关系	", "AcReconRef", "subrec=id");
            base.OnCustomDefine();
        }

        #region 框架方法重写

        /// <summary>
        /// 新增数据前
        /// 校验数据：必填、长度、唯一性
        /// </summary>
        /// <param name="data">对象</param>
        protected override void OnInsertBefore(SData data)
        {
            // 必填
            CheckEmpty(data);

            // 长度
            CheckLength(data);

            // 唯一性
            CheckUnique(data);

            base.OnInsertBefore(data);
        }

        /// <summary>
        /// 删除数据前
        /// 校验是否被引用
        /// </summary>
        /// <param name="data">对象</param>
        protected override void OnDeleteBefore(SData data)
        {
            // 判断逻辑 **********
            // * 引用删除校验
            this.CheckDeleteRef((int)data["id", 0]);

            // * 基类数据验证
            base.OnDeleteBefore(data);
        }

        /// <summary>
        /// 修改数据前
        /// 校验数据：必填、长度、唯一性
        /// </summary>
        /// <param name="OldData">修改前的对象</param>
        /// <param name="UpdateData">修改的对象</param>
        protected override void OnUpdateBefore(SData OldData, SData UpdateData)
        {
            UpdateData["id"] = UpdateData["id"] ?? OldData["id"];

            // 必填
            CheckEmpty(UpdateData, false);

            // 长度
            CheckLength(UpdateData);

            // 唯一性
            CheckUnique(UpdateData, false, OldData);

            base.OnUpdateBefore(OldData, UpdateData);
        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 校验必填项不能为空
        /// </summary>
        /// <param name="data">对象</param>
        /// <param name="isAdd">是否添加</param>
        protected virtual void CheckEmpty(SData data, bool isAdd = true)
        {
            // 校验必填
            this.CheckEmptyOrNull(data, isAdd, "dcode", "title");
        }

        /// <summary>
        /// 校验字段长度不能超出限制
        /// </summary>
        /// <param name="data">对象</param>
        protected virtual void CheckLength(SData data)
        {
            // 校验长度
            this.CheckMaxLength("dcode", 80, data);
            this.CheckMaxLength("title", 150, data);
            this.CheckMaxLength("ddesc", 300, data);
        }

        /// <summary>
        /// 校验唯一性
        /// </summary>
        /// <param name="data">对象</param>
        /// <param name="isAdd">是否添加</param>
        /// <param name="oldData">修改之前的对象</param>
        protected virtual void CheckUnique(SData data, bool isAdd = true, SData oldData = null)
        {
            if (data.ContainsKey("dcode"))
            {
                SData uniqData = new SData("dcode", data["dcode"]);
                if (!isAdd)
                {
                    uniqData.Add("id", oldData["id"]);
                }

                if (!this.IsUnique(uniqData))
                {
                    throw new Exception(((FieldDefine)ListField["dcode"]).DisplayName + this.ApiTask.LAma(AcLang.Repeat));
                }
            }
        }

        #endregion
    }
}
