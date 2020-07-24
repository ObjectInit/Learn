#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcReconRef.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcReconRef 科目对账与科目关系实体
* 创 建 者：袁炼
* 创建时间：2019/9/09 9:39:29
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System;
using System.Linq;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 科目对账与科目关系
    /// 1 描述：科目对账与科目之间关系的定义
    /// 2 约定：无
    /// 3 业务逻辑：
    ///     修改前：对账关系中被勾对记录或者单据记录所引用了科目，则不能修改该对账关系
    ///     删除前：对账关系中被勾对记录或者单据记录所引用了科目，则不能删除该对账关系
    /// </summary>
    [ClassData("cname", "科目对账与科目关系", "vision", 1)]
    public class AcReconRef : PubOthers
    {
        /// <summary>
        /// 自定义字段
        /// </summary>
        protected override void OnCustomDefine()
        {
            // 字段定义 **********
            // 启用预留字段
            this.AddField("subrec", this.ApiTask.L("科目对账"), EFieldType.关联, "AcSubRec", "a9");
            this.AddField("account", this.ApiTask.L("科目"), EFieldType.关联, "AcAccount", "a8");
            this.AddField("billtype", this.ApiTask.L("单据类型"), EFieldType.关联, "AcBillType", "a7");
            this.AddField("isreverse", this.ApiTask.L("是否反向"), EFieldType.开关, string.Empty, "a6");

            base.OnCustomDefine();
        }

        #region 框架方法重写

        /// <summary>
        /// 新增数据前
        /// 1、初始化数据
        /// 2、校验数据：必填、存在性
        /// 3、校验科目是否已加入该科目对账（即唯一性）
        /// 4、基类数据验证
        /// </summary>
        /// <param name="data">科目对账与科目关系实体</param>
        protected override void OnInsertBefore(SData data)
        {
            // 数据准备 **********
            // * 初始化数据
            data["dcode"] = data["dcode", Guid.NewGuid().ToString()];
            data["title"] = data["title", "科目对账与科目关系"];

            // 判断逻辑 **********
            // * 校验数据：必填、存在性
            // 必填
            this.CheckEmptyOrNull(data, true, "account", "subrec");

            // 存在性判断（科目对账、科目、单据类型）
            this.CheckRefExist(data, "subrec", "account", "billtype");

            // * 校验科目是否已加入该科目对账
            // 唯一性
            CheckUnique(data);

            // * 基类数据验证
            base.OnInsertBefore(data);
        }

        /// <summary>
        /// 删除数据前
        /// 1、已勾对的科目不允许删除
        /// 2、基类数据验证
        /// </summary>
        /// <param name="data">对象</param>
        protected override void OnDeleteBefore(SData data)
        {
            // 业务逻辑 **********
            // 判断原科目是否存在
            if (!string.IsNullOrEmpty(data.Item<string>("account")))
            {
                // 业务逻辑 **********
                // * 已勾对的科目不允许删除
                Validate(data, false);
            }

            // * 基类数据验证
            base.OnDeleteBefore(data);
        }

        /// <summary>
        /// 修改数据前
        /// 1、校验数据：必填、存在性
        /// 2、校验科目是否已加入该科目对账（即唯一性）
        /// 3、已勾对的科目不允许修改
        /// 4、基类数据验证
        /// </summary>
        /// <param name="OldData">修改前数据</param>
        /// <param name="UpdateData">修改后数据</param>
        protected override void OnUpdateBefore(SData OldData, SData UpdateData)
        {
            // 判断逻辑 **********
            // * 校验数据：必填、存在性
            // 必填
            this.CheckEmptyOrNull(UpdateData, false, "account", "subrec");

            // 存在性判断（科目对账、科目、单据类型）
            this.CheckRefExist(UpdateData, "subrec", "account", "billtype");

            // * 校验科目是否已加入该科目对账
            // 唯一性
            CheckUnique(UpdateData, false, OldData);

            // 业务逻辑 **********
            if (!string.IsNullOrEmpty(OldData.Item<string>("account"))) // 判断原科目是否存在
            {
                // * 已勾对的科目不允许被修改
                Validate(OldData, true);
            }
            else
            {
                throw new Exception(this.ApiTask.LEnd("原科目已被删除,不应被修改"));
            }

            // * 基类数据验证
            base.OnUpdateBefore(OldData, UpdateData);
        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 校验唯一性
        /// </summary>
        /// <param name="data">对象</param>
        /// <param name="isAdd">是否添加</param>
        /// <param name="oldData">修改之前的对象</param>
        protected virtual void CheckUnique(SData data, bool isAdd = true, SData oldData = null)
        {
            if (data.ContainsKey("account"))
            {
                if (oldData != null)
                {
                    data["subrec"] = data["subrec", oldData["subrec"]];
                }

                SData uniqData = new SData("account", data["account"], "subrec", data["subrec"]);
                if (!isAdd)
                {
                    uniqData.Add("id", oldData["id"]);
                }

                if (!this.IsUnique(uniqData))
                {
                    throw new Exception(this.ApiTask.LEnd("科目已加入该科目对账"));
                }
            }
        }

        /// <summary>
        /// 已勾对的科目不允许被修改/删除
        /// 1、定义凭证体查询业务对象
        /// 2、根据科目对账id、科目id查询勾勾兑记录、收款单、付款单的父凭证体
        /// 3、判断是否查询到凭证体数据
        /// </summary>
        /// <param name="data">科目对账与科目关系对象</param>
        /// <param name="isUpdate">true:更新校验,false:删除校验</param>
        protected void Validate(SData data, bool isUpdate)
        {
            // 定义凭证体查询业务对象
            var dbiz = this.ApiTask.Biz<BizTable>("AcVoucherDSub");

            // 根据科目对账id、科目id查询勾勾兑记录、收款单、付款单的父凭证体
            var dlist = dbiz.GetListData(1, 1, new SData("subused", $"{data["subrec.id"]}.{data["account.id"]}").toParmStr(), "id");

            // 判断是否查询到凭证体数据
            if (dlist != null && dlist.Count > 0)
            {
                throw new Exception(isUpdate ? this.ApiTask.LEnd("科目已对账，不允许修改") : this.ApiTask.LEnd("科目已对账，不允许删除"));
            }
        }

        #endregion
    }
}
