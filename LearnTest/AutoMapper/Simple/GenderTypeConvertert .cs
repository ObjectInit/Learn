#region  <<版本注释>>
/* ========================================================== 
// <copyright file="GenderTypeConvertert.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：GenderTypeConvertert 
* 创 建 者：Administrator 
* 创建时间：2019/11/21 17:13:49 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Learn.Console.AutoMapper.Simple
{

    /// <summary>
    /// 源到目标  类型的转换
    /// </summary>
    public class GenderTypeConvertert: ITypeConverter<int,string>
    {
        public string Convert(int source, string destination, ResolutionContext context)
        {
            switch (source)
            {
                case 1:
                    destination = "男";
                    break;
                case 2:
                    destination = "女";
                    break;
                default :
                    destination = "未知";
                    break;
            }

            return destination;
        }
    }
}
