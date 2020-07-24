#region  << 版 本 注 释 >>
/* ============================================================================== 
// <copyright file="ConstResourceDB.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：ConstResourceDB 
* 创 建 者：龚绍平
* 创建时间：2020/1/7 22:51:08 
* ==============================================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJ.BackEnd.Base;
using SJ.Global;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 数据库静态常量（包含Id和Dcode）
    /// </summary>
    public static class ConstResourceDB
    {
        private static SData ConstResouce = new SData();

        /// <summary>
        /// 状态、类别等其他代码对象缓存（存储在others表中）
        /// </summary>
        /// <param name="apiTask">apitask</param>
        /// <param name="dclass">类名称</param>
        /// <param name="dcode">代码</param>
        /// <returns></returns>
        public static int GetId(ApiTask apiTask, string dclass, string dcode)
        {
            // 变量声明
            var key = $"{apiTask.Domain}_{dclass}"; // 将账套以及类代码作为key存储起来
            if (!ConstResouce.ContainsKey(key))
            {
                ConstResouce.Add(key, new SData());
                var listData = apiTask.DB.ExecuteList(@"select id,dcode from others where dclass='" + dclass.Trim() + "'");
                listData.ForEach(m =>
                {
                    ((SData)ConstResouce[key]).Append(m.Item<string>("dcode"), m.Item<int>("id"));
                });
            }

            return ((SData)ConstResouce[key]).Item<int>(dcode);
        }

        /// <summary>
        /// 获取科目（如果已缓存则直接返回，没有的话则通过数据库查询）
        /// </summary>
        /// <param name="apiTask">apitask</param>
        /// <param name="dcode">科目代码</param>
        /// <returns></returns>
        public static int GetAccount(ApiTask apiTask, string dcode)
        {
            // 变量声明
            var key = $"{apiTask.Domain}_account"; // 通过账套及科目代码进行缓存
            if (ConstResouce.ContainsKey(key))
            {
                return ((SData)ConstResouce[key]).Item<int>(dcode);
            }

            ConstResouce.Add(key, new SData());
            var listData = apiTask.DB.ExecuteList(@"select id,dcode from account");
            listData.ForEach(m =>
            {
                ((SData)ConstResouce[key]).Append(m.Item<string>("dcode"), m.Item<int>("id"));
            });
            return ((SData)ConstResouce[key]).Item<int>(dcode);
        }
    }
}