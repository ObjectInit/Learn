#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcYearEndA.cs" company="Shiji.BO.CS">
// Copyright (c) SJ.BO.CS. All rights reserved.
// </copyright>
* 功能描述：自动凭证结转与科目
* 创 建 者：张莉
* 创建时间：2019/9/30 9:23:40
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 自动凭证结转与科目
    /// 1 描述：定义凭证需要转出的科目和转入的科目
    /// 2 约定：无
    /// 3 业务逻辑：
    ///     新增前：结转余额比例数值校验，转出科目不能与转入科目相同
    ///     修改前：结转余额比例数值校验，转出科目不能与转入科目相同，转入科目被删除或被修改时清除转入科目分析项、币种
    ///     查询前：列表按照转出科目性质、转出科目升序排序
    /// </summary>
    [ClassData("cname", "自动凭证结转与科目", "vision", 1)]
    public class AcYearEndA : BizTableCode
    {
        public override string TableName
        {
            get => "account";
            set => TableName = "account";
        }

        /// <summary>
        /// 字段定义、查询参数定义、子查询定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // * 字段定义
            // 启用预留字段
            this.AddField("outacc", ApiTask.L("转出科目"), EFieldType.关联, "AcAccount", "a19");
            this.AddField("inacc", ApiTask.L("转入科目"), EFieldType.关联, "AcAccount", "a18");
            this.AddField("yearend", ApiTask.L("自动凭证结转"), EFieldType.关联, nameof(AcYearEnd), "a17");
            this.AddField("arate", ApiTask.L("结转余额比例"), EFieldType.数值, string.Empty, "d0");

            this.AddField("property", ApiTask.L("转出科目性质"), EFieldType.关联, "AcAcProperty", "property");
            this.AddField("currency", ApiTask.L("币种"), EFieldType.关联, "AcCurrency", "currency");

            // 转入科目分析项tcode
            var data = ApiTask.Tcode();
            foreach (var item in data)
            {
                if (!item.Key.Contains(AcConst.RecoTcode))
                {
                    this.AddField(item.Key, item.Value.ToString().Sp_Last(), EFieldType.关联, item.Value.ToString().Sp_First(), item.Key);
                }
            }
        }

        #region 框架方法重写

        /// <summary>
        /// 自动凭证结转与科目新增前处理的逻辑：
        /// 1、初始化新增数据
        /// 2、数据格式验证
        /// 3、存在性验证及逻辑验证
        /// </summary>
        /// <param name="data">自动凭证结转与科目实体</param>
        protected override void OnInsertBefore(SData data)
        {
            if (data != null)
            {
                // 参数定义 **********
                // * 初始化新增数据
                data["dcode"] = Guid.NewGuid().ToString();
                data["title"] = "自动凭证结转与科目";

                // 判断逻辑 **********
                // * 数据格式验证
                Valid(data);

                // 业务逻辑 **********
                // * 存在性验证及逻辑验证
                CheckExistOrLogic(data);

                base.OnInsertBefore(data);
            }
        }

        /// <summary>
        /// 自动凭证结转与科目修改前处理的逻辑：
        /// 1、初始化修改数据
        /// 2、数据格式验证
        /// 3、存在性验证及逻辑验证
        /// </summary>
        /// <param name="oldData">修改前数据</param>
        /// <param name="updateData">修改后数据</param>
        protected override void OnUpdateBefore(SData oldData, SData updateData)
        {
            // 参数定义 **********
            // * 初始化修改数据
            updateData["id"] = updateData["id"] ?? oldData["id"];
            if (!updateData.ContainsKey("property"))
            {
                updateData["property"] = oldData["property"];
            }

            if (!updateData.ContainsKey("outacc"))
            {
                updateData["outacc"] = oldData["outacc"];
            }

            // 判断逻辑 **********
            // * 数据格式验证
            Valid(updateData, false);

            // 业务逻辑 **********
            // * 存在性验证及逻辑验证
            CheckExistOrLogic(updateData, oldData);

            // * 转入科目 【被删除】或【被修改】，需要同步将对应的设置的转入科目分析项、币种清除
            var oldInacc = oldData.Item<string>("inacc");
            var updateInacc = updateData.Item<string>("inacc");
            if (!string.IsNullOrEmpty(oldInacc) && updateData.ContainsKey("inacc") && !updateInacc.Sp_First().Equals(oldInacc.Sp_First()))
            {
                updateData["currency"] = null;

                var data = this.ApiTask.Tcode();
                foreach (var t in data)
                {
                    updateData[t.Key] = null;
                }
            }

            base.OnUpdateBefore(oldData, updateData);
        }

        /// <summary>
        /// 自动凭证结转与科目查询前处理的逻辑：
        /// 1、定义的科目按照转出科目代码升序排序
        /// </summary>
        /// <param name="qParm">查询参数</param>
        protected override void OnGetListBefore(SData qParm)
        {
            base.OnGetListBefore(qParm);

            // 业务逻辑 **********
            // * 定义的科目按照转出科目代码升序排序
            qParm.Add("q.orderby", "property.dcode,outacc.dcode");
        }
        #endregion

        #region 自定义方法

        /// <summary>
        /// 验证数据格式
        /// <param name="data">自动凭证结转与科目实体</param>
        /// <param name="isInsert">true是新增,false是修改</param>
        /// </summary>
        private void Valid(SData data, bool isInsert = true)
        {
            // 转出科目性质、转出科目不能同时为空
            if (string.IsNullOrEmpty(data.Item<string>("property")) && string.IsNullOrEmpty(data.Item<string>("outacc")))
            {
                throw new Exception($"{((FieldDefine)ListField["property"]).DisplayName}{ApiTask.L("、")}{((FieldDefine)ListField["outacc"]).DisplayName}{ApiTask.LEnd("不能同时为空")}");
            }

            // 校验必填
            this.CheckEmptyOrNull(data, isInsert, "arate", "yearend");

            // 结转余额比例四舍五入，保留2位小数，且 结转余额比例为大于0的数值
            if (data.ContainsKey("arate"))
            {
                decimal.TryParse(data["arate"].ToString(), out decimal arate);
                if (arate <= 0)
                {
                    throw new Exception($"{((FieldDefine)ListField["arate"]).DisplayName}{ApiTask.LEnd("为大于0的数值")}");
                }

                arate = data["arate"].ToString().ToDec();
                data["arate"] = arate;

                if (arate <= 0)
                {
                    throw new Exception($"{((FieldDefine)ListField["arate"]).DisplayName}{ApiTask.LEnd("为大于0的数值")}");
                }
            }
        }

        /// <summary>
        /// 存在性验证及逻辑验证
        /// </summary>
        /// <param name="data">新增或修改的自动凭证结转科目实体</param>
        /// <param name="oldData">修改前的自动凭证结转科目实体</param>
        private void CheckExistOrLogic(SData data, SData oldData = null)
        {
            var acYearEndA = ApiTask.Biz<BizTableCode>(nameof(AcAccount));

            if (oldData != null)
            {
                // 转入科目有值时，判断转出科目不能与转入科目相同
                data["outacc"] = data["outacc", oldData["outacc"]];
                data["inacc"] = data["inacc", oldData["inacc"]];
            }

            // * 存在性验证：自动凭证结转、转出科目性质、转出科目、转入科目
            this.CheckRefExist(data, "yearend", "property", "outacc", "inacc");

            // 如果转入科目有值，则设置转入科目的分析项、币种
            if (!string.IsNullOrEmpty(data.Item<string>("inacc")))
            {
                // 币种是否存在
                this.CheckRefExist(data, "currency");

                // tcode是否存在
                var inacc = acYearEndA.GetIDAuto(data["inacc"].ToString());
                var inaccItem = acYearEndA.GetItem(inacc);
                AcVoucherHelper.CheckTcode(this.ApiTask, inaccItem, data, false);
            }

        }

        #endregion
    }
}
