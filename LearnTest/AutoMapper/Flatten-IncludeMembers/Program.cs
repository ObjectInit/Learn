#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2019/8/8 11:47:05 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Learn.Console.AutoMapper.Simple;

namespace Learn.Console.AutoMapper.Flatten_IncludeMembers
{
    /// <summary>
    /// 源类型
    /// 包含了 子对象
    /// </summary>
    class Source
    {
        public string Name { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
        public InnerSource InnerSource { get; set; }

         
        
    }

    /// <summary>
    /// 目标类型
    /// 需要从目标类型的子对象去取
    /// </summary>
    class Destination
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }

    class InnerSource
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string Title1 { get; set; }
    }
    class OtherInnerSource
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
   

    public class Program : IMain
    {
        public void Main(string[] args)
        {
            SingleMapWithIncludeMembers3();
        }

        /// <summary>
        /// 
        /// </summary>
        private void SingleMap()
        {
            //在目标类型的属性未按照约定命名时候,映射失效 需要指定规则
            MapperConfiguration config = new MapperConfiguration(
                cfg =>
                {
                    cfg.CreateMap<Source, Destination>();

                }
            );
            var mapper = config.CreateMapper(); 
            var source = new Source
            {
                Name = "name",
                InnerSource = new InnerSource { Description = "description" },
                OtherInnerSource = new OtherInnerSource { Title = "title" }
            };
            var destination = mapper.Map<Destination>(source);
        }


        private void SingleMapWithIncludeMembers()
        {
            MapperConfiguration config = new MapperConfiguration(
                cfg =>
                {
                    cfg.CreateMap<Source, Destination>().IncludeMembers(x=>x.InnerSource);//配置目标类型能从 源类型能查找的子对象类型
                    cfg.CreateMap<InnerSource, Destination>();//配置目标类型与源目标子对象类型映射
                }
            );
            var mapper = config.CreateMapper();
            var source = new Source
            {
                Name = "name",
                InnerSource = new InnerSource { Description = "description" },
                OtherInnerSource = new OtherInnerSource { Title = "title" }
            };
            var destination = mapper.Map<Destination>(source); //目标类型 会去按照配置的规则查找匹配属性
        }

        private void SingleMapWithIncludeMembers2()
        {
            //此示例中 目标类型的属性可以从多个源类型中匹配到时
            MapperConfiguration config = new MapperConfiguration(
                cfg =>
                {
                    cfg.CreateMap<Source, Destination>().IncludeMembers(x => x.OtherInnerSource, x=>x.InnerSource);//配置目标类型能从 源类型能查找的子对象类型
                    cfg.CreateMap<OtherInnerSource, Destination>();
                    cfg.CreateMap<InnerSource, Destination>();//配置目标类型与源目标子对象类型映射,如果源类型包含多个子对象,需要创建多个到目标类型的映射
                }
            );
            var mapper = config.CreateMapper();
            var source = new Source
            {
                Name = "name",
                InnerSource = new InnerSource { Description = "description" },
                OtherInnerSource = new OtherInnerSource {Description= "description1", Title = "title",Name="name1" }
            };
            //目标类型 会去按照配置的规则查找匹配属性
            //如果匹配到多个源类型都有目标类型的属性,则会按照IncludeMembers配置的顺序查找到的一个
            var destination = mapper.Map<Destination>(source); 
        }

        private void SingleMapWithIncludeMembers3()
        {
            MapperConfiguration config = new MapperConfiguration(
                cfg =>
                {
                    cfg.CreateMap<Source, Destination>(MemberList.Source)
                    .IncludeMembers(x => x.InnerSource,x=>x.OtherInnerSource);//配置目标类型能从 源类型能查找的子对象类型
                    cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
                    cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(des=>des.Title,o=>o.MapFrom(x=>x.Title1));//配置目标类型与源目标子对象类型映射,如果源类型包含多个子对象,需要创建多个到目标类型的映射
                }
            );
            var mapper = config.CreateMapper();
            config.AssertConfigurationIsValid();
            var source = new Source
            {
                Name = "name",
                InnerSource = new InnerSource { Description = "description",Title1="Title1" },
                OtherInnerSource = new OtherInnerSource { Description = "description1", Title = "title", Name = "name1" }
            };
            var destination = mapper.Map<Destination>(source);
        }
    }
}
