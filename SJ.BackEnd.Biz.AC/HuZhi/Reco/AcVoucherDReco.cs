#region << 版 本 注 释 >>
/* ==============================================================================
// <copyright file="AcVoucherDCustom.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：凭证体自定义查询
* 创 建 人：胡智
* 创建日期：2019-11-20 15:41:22
* ==============================================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 凭证体自动勾对查询
    /// 1.描述：用于查询“从未勾对的数据”
    /// 2.约定：
    ///     1.查询时使用autoreco标识查询从未勾对的数据,值传科目对账Id
    /// 3.业务逻辑：
    ///     查询前：查询“从未勾对数据”
    ///  </summary>
    [ClassData("cname", "凭证分录")]
    public class AcVoucherDReco : AcVoucherD
    {
        /// <summary>
        /// 参数、子查询定义
        /// </summary>
        protected override void OnCustomDefine()
        {
            base.OnCustomDefine();

            // * 查询参数定义
            this.AddParm("autoreco", "从未勾对", EFieldType.字符串);
        }

        /// <summary>
        /// 自定义查询参数的sql处理
        /// 1.处理从未勾对的查询参数
        /// </summary>
        /// <param name="parmKey">参数key</param>
        /// <param name="parmStr">参数value(科目对账Id)</param>
        /// <returns>自定义sql</returns>
        protected override string CustomParmSql(string parmKey, string parmStr)
        {
            string sql = base.CustomParmSql(parmKey, parmStr);

            // * 处理从未勾对的查询参数
            if (parmKey == "autoreco")
            {
                var recoAccountId = ConstResourceDB.GetAccount(this.ApiTask, AcConst.RecoAccount);

                // 草稿和待审的收付款单，自动勾对不可以勾对；草稿和待审的收付款单，手动勾对不可以勾对
                sql += $@" 
not exists (select parent 
from 
voucherd 
where 
account={recoAccountId} and 
{AcConst.RecoTcode}={parmStr} and 
parent={{0}}.id)";

                // 获取科目对账中启用的分析项
                List<string> tcodeList = GetTcodes(int.Parse(parmStr));

                // 添加设置的tcode不为空的条件
                foreach (var t in tcodeList)
                {
                    sql += $@" and 
{{0}}.{t} is not null ";
                }
            }

            return sql;
        }

        /// <summary>
        /// 根据科目对账获取科目对账设置的Tcode
        /// 1.获取科目对账
        /// 2.找到启用的Tcode
        /// </summary>
        /// <param name="subrec">科目对账Code</param>
        /// <returns>返回科目对账设置的Tcode</returns>
        private List<string> GetTcodes(int subrec)
        {
            // 数据准备 **********
            // 变量声明
            List<string> tCodeList = new List<string>(); // 保存科目对账设置的Tcode

            // * 获取科目对账
            var acSubRec = ApiTask.Biz<BizTable>("AcSubRec").GetItem(subrec);

            // 业务逻辑 **********
            // * 找到启用的Tcode
            var tcodeValueList = acSubRec.Where(x => Regex.IsMatch(x.Key, Regx.TcodeModel) && x.Value.Equals(true));

            if (tcodeValueList.Any())
            {
                foreach (var item in tcodeValueList)
                {
                    tCodeList.Add(item.Key);
                }
            }

            return tCodeList;
        }
    }
}
