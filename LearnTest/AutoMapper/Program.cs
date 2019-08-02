#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2019/8/2 13:24:39 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Learn.Console.AutoMapper
{
    public class Program : IMain
    {
        public void Main(string[] args)
        {
            SinglerMapper();
        }


        /// <summary>
        /// 简单数据映射
        /// </summary>
        /// <param name="mapper"></param>
        private void SinglerMapper()
        {
            //1.创建mapper对象
            MapperConfiguration config = new MapperConfiguration(
                cfg => cfg.CreateMap<User, UserDto>()
            );
            var mapper = config.CreateMapper();
            //// or
            //var mapper = new Mapper(config);

            //1.定义源数据
            User user = new User
            {
                Name = "刘文汉",
                Id = 1
            };

            //定义目标数据
            User user2 = new User
            {
                BookCount = 12,
                Age = 12
            };
            //结论 默认automapper会把源目标数据映射到目标数据上，即使是null 或者0 数据
            var newUser = mapper.Map(user, user2);
        }
    }
}
