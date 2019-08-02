#region  <<版本注释>>
/* ========================================================== 
// <copyright file="MappingProfile.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：MappingProfile 
* 创 建 者：Administrator 
* 创建时间：2019/8/2 15:50:14 
* =============================================================*/
#endregion

using AutoMapper;

namespace Learn.Console.AutoMapper.Simple
{
    /// <summary>
    /// 直接通过构造函数进行配置
    /// 映射配置
    /// </summary>
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
        }
    }
}
