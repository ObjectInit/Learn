#region  << 版 本 注 释 >>
/* ==============================================================================
// <copyright file="CurrentUserHelper.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：CurrentUserHelper
* 创 建 者：龚绍平
* 创建时间：2019/11/1 14:00:35
* ==============================================================================*/
#endregion
using SJ.BackEnd.Base;
using SJ.Global;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// apitask扩展方法
    /// 1 描述: 根据上下文获取当前用户的核算单位
    /// 2 约定：默认使用subunit作为核算单位的key，如果实际场景不符合，则指定key
    /// 3 业务逻辑：
    ///     当系统启用多核算单位结转时：如果是开发或者用户的核算单位为空，则不做核算单位过滤；否则以当前用户的核算单位作为过滤条件
    ///     当系统不允许多核算单位结转时：如果是开发或者用户的核算单位为空，以总部作为过滤条件；否则以当前用户的核算单位作为过滤条件
    /// </summary>
    public static class CurrentUserHelper
    {
        /// <summary>
        /// 核算单位处理
        /// <para>默认赋值业务数据的核算单位为当前用户的核算单位，如果核算单位为空，则拥有所有核算单位权限;</para>
        /// </summary>
        /// <param name="apiTask">当前会话</param>
        /// <param name="sData">查询参数或者新增参数</param>
        /// <param name="subunitKey">核算单位属性名，默认为subunit</param>
        public static void SubUnitProcess(this ApiTask apiTask, SData sData, string subunitKey = "subunit")
        {
            // 数据准备 **********
            var user = apiTask.UserInfo();

            // 业务逻辑 **********
            // 当前用户核算单位不为空，添加核算单位条件
            if (string.IsNullOrEmpty(sData.Item<string>(subunitKey)))
            {
                if (user.UserType() == EUserType.Developer || string.IsNullOrEmpty(user.UserSubUnit())) // 所有权限
                {
                    var defaultUnit = apiTask.GetParms("subunit", 0) == "1" ? string.Empty : AcConst.UnitHead;
                    sData.Append(subunitKey, defaultUnit);
                }
                else
                {
                    sData.Append(subunitKey, user.UserSubUnit());
                }
            }
            else // 前端传入了校验参数是否正确
            {
                if (apiTask.GetParms("subunit", 0) == "0") // 不允许多核算单位结转
                {
                    sData.Append(subunitKey, AcConst.UnitHead); // 只能是总部
                }
                else
                {
                    if (user.UserType() != EUserType.Developer && !string.IsNullOrEmpty(user.UserSubUnit())) // 所有权限
                    {
                        if (user.UserSubUnit().Sp_First() != sData.Item<string>(subunitKey)) // 如果传入的核算单位不属于自己的核算单位范围，则以用户的核算单位为准
                        {
                            sData.Append(subunitKey, user.UserSubUnit());
                        }
                    }
                }
            }
        }
    }
}