#region  << 版 本 注 释 >>
/* ==============================================================================
// <copyright file="PeriodHelper.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：PeriodHelper
* 创 建 者：GONG
* 创建时间：2019/9/30 10:43:53
* ==============================================================================*/
#endregion
using SJ.BackEnd.Base;
using System;
using System.Text.RegularExpressions;
using SJ.BackEnd.Biz.Pub;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 核算期
    /// 1 描述：核算期辅助类
    /// 2 约定：目前不校验核算期的最大跨区间数
    /// 3 业务逻辑：
    ///     1、核算期校验
    ///     2、获取当前核算期
    /// </summary>
    public class PeriodHelper
    {
        /// <summary>
        /// 核算期校验
        /// </summary>
        /// <param name="period">核算期值</param>
        /// <param name="apiTask">实体的工作会话，this.ApiTask</param>
        public static void Check(int period, ApiTask apiTask)
        {
            // 数据准备 **********
            // 得到系统当前核算期
            var currentPeriod = apiTask.GetParms(AcConst.PeriodCurrent);

            // 得到每年核算期数
            var periodNum = apiTask.GetParms(AcConst.PeriodNum);

            int.TryParse(periodNum, out var periodMaxNum);

            // 判断逻辑 **********
            if (!Regex.IsMatch(period.ToString(), Regx.Period))
            {
                throw new Exception($"{apiTask.L(AcLang.Period)}{apiTask.LEnd(PubLang.FormartError)}");
            }

            // 业务逻辑 **********
            // 验证核算期不能大于每年最大核算期数，每年最大核算期数为系统参数中设置的最大核算期
            if (int.Parse(period.ToString().Substring(4, 2)) > periodMaxNum)
            {
                throw new Exception(apiTask.LEnd("核算期不能大于每年最大核算期数"));
            }

            // 验证核算期不能小于当前核算期
            if (period < int.Parse(currentPeriod))
            {
                throw new Exception(apiTask.LEnd("核算期不能小于当前核算期"));
            }
        }

        /// <summary>
        /// 获取当前核算期
        /// </summary>
        /// <param name="apiTask">实体的工作会话，this.ApiTask</param>
        /// <returns>当前核算期</returns>
        public static int GetCurrent(ApiTask apiTask)
        {
            // 得到系统当前核算期
            var currentPeriod = apiTask.GetParms(AcConst.PeriodCurrent);

            return int.Parse(currentPeriod);
        }
    }
}