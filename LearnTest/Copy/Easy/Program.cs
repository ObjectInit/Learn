#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2019/11/22 10:23:16 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Console.Copy.Easy
{
    public class Program:IMain
    {
        public void Main(string[] args)
        {
            List<PersonA> personList = new List<PersonA>()
            {
                new PersonA() { Name="PersonA", Age= 10, ClassA= new A() { TestProperty = "AProperty"} },
                new PersonA() { Name="PersonA2", Age= 20, ClassA= new A() { TestProperty = "AProperty2"} }
            };
            // 下面2种方式实现的都是浅拷贝
            List<PersonA> personsCopy = new List<PersonA>(personList);
            
            PersonA[] personCopy2 = new PersonA[2];
            personList.CopyTo(personCopy2);

            // 由于实现的是浅拷贝，所以改变一个对象的值，其他2个对象的值都会发生改变，因为它们都是使用的同一份实体，即它们指向内存中同一个地址　
            personsCopy.First().ClassA.TestProperty = "AProperty3";

        }

        class ShallowCopyDemoClass : ICloneable
        {
            public int intValue = 1;
            public string strValue = "1";
            public PersonEnum pEnum = PersonEnum.EnumA;
            public PersonStruct pStruct = new PersonStruct() { StructValue = 1 };
            public Person pClass = new Person("1");
            public int[] pIntArray = new int[] { 1 };
            public string[] pStringArray = new string[] { "1" };

            #region ICloneable成员
            public object Clone()
            {
                return this.MemberwiseClone();
            }

            #endregion

        }

        class Person
        {
            public string Name;
            public Person(string name)
            {
                Name = name;
            }
        }
        public enum PersonEnum
        {
            EnumA = 0,
            EnumB = 1
        }

        public struct PersonStruct
        {
            public int StructValue;
        }
    }

    public class A
    {
        public string TestProperty { get; set; }
    }

    public class PersonA
    {
        public int Age { get; set; }
        public string Name { get; set; }
        public A ClassA { get; set; }
    }
}
