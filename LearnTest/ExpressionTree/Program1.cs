#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program1.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program1 
* 创 建 者：Administrator 
* 创建时间：2019/12/11 15:57:44 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Console.ExpressionTree
{
    public class Program1 : IMain
    {
        public void Main(string[] args)
        {
            SimpleExpress();
        }

        /// <summary>
        /// 表达式的创建-解析-编译
        /// </summary>
        public void SimpleExpress()
        {
            // 通过api创建表达式树 num<5
            ParameterExpression pInt = Expression.Parameter(typeof(int), "num");
            ConstantExpression five = Expression.Constant(5,typeof(int));
            BinaryExpression numLessThanFive = Expression.LessThan(pInt, five);

            // 创建lambda表达式 (num,bool)=>num<5
            Expression<Func<int, bool>> labda1 = Expression.Lambda<Func<int, bool>>(numLessThanFive, new ParameterExpression[]{ pInt }  );

            labda1.Compile().Invoke(3);
        }

        /// <summary>
        /// 表达式的遍历
        /// </summary>
        public void ExpressForeach()
        {

        }
    }

    
}
