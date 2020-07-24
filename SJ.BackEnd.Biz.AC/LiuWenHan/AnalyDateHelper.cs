#region  <<版本注释>>
/* ========================================================== 
// <copyright file="AnalyDateHelper.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AnalyDateHelper 
* 创 建 者：Administrator 
* 创建时间：2020/7/9 17:22:20 
* =============================================================*/
#endregion
using SJ.BackEnd.Base;
using SJ.Global;
using System;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 账龄分析帮助类
    /// </summary>
    public class AnalyDateHelper
    {
        /// <summary>
        /// 按核算期进行账龄分析
        /// </summary>
        public static bool AusePeriod(ApiTask api)
        {
            return api.GetParms(AcConst.AusePeriod) == "1";
        }

        /// <summary>
        /// 获取账龄分析日期区间(根据分析日期)
        /// </summary>
        /// <param name="analysdate">分析日期</param>
        /// <param name="acAnalyData">账龄对象</param>
        /// <param name="startDate">输出 的 开始时间</param>
        /// <param name="endDate">输出 的 结束时间</param>
        public static void GetAnalyDate(DateTime analysdate, SData acAnalyData, out DateTime startDate, out DateTime endDate)
        {
            startDate = analysdate;
            endDate = analysdate;
            if (acAnalyData != null)
            {
                int.TryParse(acAnalyData["begins"]?.ToString(), out int begins);
                int.TryParse(acAnalyData["ends"]?.ToString(), out int ends);

                if (acAnalyData["acanalytype"].ToString().Sp_First().Equals(AcConst.ToExpire))
                {
                    if (acAnalyData["begintype"].ToString().Sp_First().Equals(AcConst.Day))
                    {
                        startDate = startDate.AddDays(begins);
                    }
                    else if (acAnalyData["begintype"].ToString().Sp_First().Equals(AcConst.Month))
                    {
                        startDate = startDate.AddMonths(begins);
                    }

                    if (acAnalyData["endtype"].ToString().Sp_First().Equals(AcConst.Day))
                    {
                        endDate = endDate.AddDays(ends).AddDays(-1);
                    }
                    else if (acAnalyData["endtype"].ToString().Sp_First().Equals(AcConst.Month))
                    {
                        endDate = endDate.AddMonths(ends).AddDays(-1);
                    }
                }
                else
                {
                    if (acAnalyData["begintype"].ToString().Sp_First().Equals(AcConst.Day))
                    {
                        endDate = endDate.AddDays(-begins).AddDays(-1);
                    }
                    else if (acAnalyData["begintype"].ToString().Sp_First().Equals(AcConst.Month))
                    {
                        endDate = endDate.AddMonths(-begins).AddDays(-1);
                    }

                    if (acAnalyData["endtype"].ToString().Sp_First().Equals(AcConst.Day))
                    {
                        startDate = startDate.AddDays(-ends);
                    }
                    else if (acAnalyData["endtype"].ToString().Sp_First().Equals(AcConst.Month))
                    {
                        startDate = startDate.AddMonths(-ends);
                    }
                }
            }
        }

        /// <summary>
        /// 获取账龄分析日期区间(根据核算期)
        /// </summary>
        /// <param name="period">核算期</param>
        /// <param name="acAnalyData">账龄对象</param>
        /// <param name="startPeriod">输出 的 核算期开始时间</param>
        /// <param name="endPeriod">输出 的 核算期结束时间</param>
        /// <param name="periodPerYear">每年核算期数</param>
        public static void GetAnalyDate(int period, SData acAnalyData, out int startPeriod, out int endPeriod, int periodPerYear)
        {
            startPeriod = period;
            endPeriod = period;
            int periodPerYearV = periodPerYear;
            if (acAnalyData != null)
            {
                int.TryParse(acAnalyData["begins"]?.ToString(), out int begins);
                int.TryParse(acAnalyData["ends"]?.ToString(), out int ends);

                if (acAnalyData["acanalytype"].ToString().Sp_First().Equals(AcConst.ToExpire))
                {
                    startPeriod = AddPeriod(startPeriod, begins, periodPerYearV);
                    endPeriod = AddPeriod(endPeriod, ends - 1, periodPerYearV);
                }
                else
                {
                    endPeriod = AddPeriod(endPeriod, -(begins + 1), periodPerYearV);
                    startPeriod = AddPeriod(startPeriod, -ends, periodPerYearV);
                }
            }
        }

        /// <summary>
        /// 核算期计算
        /// </summary>
        /// <param name="period">当前核算期</param>
        /// <param name="periods">核算期的叠加数</param>
        /// <param name="periodPerYear">每年核算期数</param>
        /// <returns></returns>
        public static int AddPeriod(int period, int periods, int periodPerYear)
        {
            int periodYear = Convert.ToInt32(period.ToString().Substring(0, 4));
            int periodMonths = Convert.ToInt32(period.ToString().Substring(4, 2));
            int months = periodMonths + periods;
            if (months <= 0)
            {
                periodYear = periodYear - 1;
                periodYear = periodYear + (months / periodPerYear);
                periodMonths = periodPerYear + (months % periodPerYear);
            }
            else
            {
                periodYear = periodYear + (months / periodPerYear);
                periodMonths = months % periodPerYear;

                // 201901 + 24
                if (months % periodPerYear == 0)
                {
                    periodMonths = periodPerYear;
                    periodYear = periodYear - 1;
                }
            }

            return (periodYear * 100) + periodMonths;
        }
    }
}
