#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2019/7/2 15:43:13 
* =============================================================*/
#endregion

using System.Collections;
using System.Collections.Generic;

namespace Learn.Console.MicrosoftFrameWork.KnowForeach
{
    public class Program : IMain
    {
        public void Main(string[] args)
        { 
            #region foreach原理初探
            //任何类型，只要想使用foreach来循环遍历，就必须在当前类型中存在：
            //public IEnumerator GetEnumerator()方法，（一般情况我们会通过实现IEnumerable接口，来创建该方法。）
            
            Person p = new Person();
            p[0] = "BMW";
            p[1] = "凯迪拉克";
            p[2] = "阿斯顿马丁";
             
            foreach (var item in p)
            {
                System.Console.WriteLine(item);
            }
            ////foreach循环原理遍历输出
            //Console.WriteLine("=======foreach循环原理遍历输出======");
            //IEnumerator etor = p.GetEnumerator();
            //while (etor.MoveNext())
            //{
            //    string str = etor.Current.ToString();
            //    Console.WriteLine(str);
            //}
            #endregion 
            System.Console.ReadKey();
 
        }
    }

    public class Person : IEnumerable
    {
        public List<string> listCar = new List<string> { };

        public int Count
        {
            get { return listCar.Count; }
        }

        public string Name { get; set; }

        public int Age { get; set; }

        public string Email { get; set; }


        public string this[int index]
        {
            get { return listCar[index]; }
            set
            {
                if (index >= listCar.Count)
                {
                    listCar.Add(value);
                }
                else
                {
                    listCar[index] = value;
                }
            }
        }

        //这个方法的作用不是用来遍历的，而是用来获取 一个对象
        //这个对象才是用来遍历的   
        public IEnumerator GetEnumerator()
        {
            return new PersonIenumerator(listCar);
        }
    }

    /// <summary>
    /// 这个类型的作用就是 用来遍历Person中的List集合的
    /// </summary>
    public class PersonIenumerator : IEnumerator
    {
        private List<string> cars;
        private int index=-1;
        public PersonIenumerator(List<string> cars)
        {
            this.cars = cars;
        }
        public bool MoveNext()
        {
            index = index + 1;
            if (index >= cars.Count) return false;
            return true;
        }

        public void Reset()
        {
            index = -1;
        }

        public object Current
        {
            get
            {
                if (index < 0) return null;
                return cars[index];
            }
        }
    }
}
