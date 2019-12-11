#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2019/12/11 13:49:54 
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
    public class Program : IMain
    {
        public void Main(string[] args)
        {
            CreateComplex();
            System.Console.ReadLine();
        }

        /// <summary>
        /// 创建简单的表达式树
        /// </summary>
        public void CreateSimple()
        {
            Expression<Func<int, int>> exp = (x) => x + 1;
            System.Console.WriteLine(exp.ToString());  // x=> (x + 1)

            var lambdaExpr = (LambdaExpression)exp; //Expression<TDelegate>是直接继承自LambdaExpression
            System.Console.WriteLine(lambdaExpr.Body);
            System.Console.WriteLine(lambdaExpr.ReturnType.ToString());

            foreach (var parameter in lambdaExpr.Parameters)
            {
                System.Console.WriteLine("Name:{0}, Type: {1}, ", parameter.Name, parameter.Type.ToString());
            }
        }

        /// <summary>
        /// 创建较复杂的表达式树
        /// </summary>
        public void CreateComplex()
        {
            // 下面的方法编译不能过
            //Expression<Action> lambdaExpression2 = () =>
            //{
            //    for (int i = 1; i <= 10; i++)
            //    {
            //       System.Console.WriteLine("Hello");
            //    }
            //};

            #region 多执行块
            //上面我们先写了一个LoopExpression，然后把它传给了BlockExpresson，
            //从而形成的的一块代码或者我们也可以说一个方法体。
            //但是如果我们有多个执行块，而且这多个执行块里面需要处理同一个参数，我们就得在block里面声明这些参数了
            ParameterExpression number = Expression.Parameter(typeof(int), "number");

            BlockExpression myBlock = Expression.Block(
                new[] { number },
                Expression.Assign(number, Expression.Constant(2)),
                Expression.AddAssign(number, Expression.Constant(6)),
                Expression.DivideAssign(number, Expression.Constant(2)));

            Expression<Func<int>> myAction = Expression.Lambda<Func<int>>(myBlock);
            System.Console.WriteLine(myAction.Compile()());
            // (2+6) / 2 = 4
            #endregion
             
        }

        /// <summary>
        /// loop 跳出
        /// </summary>
        public void CreateLoopBreak()
        {
            LabelTarget labelBreak = Expression.Label();
            ParameterExpression loopIndex = Expression.Parameter(typeof(int), "index");

            BlockExpression block = Expression.Block(
                new[] { loopIndex },
                // 初始化loopIndex =1
                Expression.Assign(loopIndex, Expression.Constant(1)),
                Expression.Loop(
                    Expression.IfThenElse(
                        // if 的判断逻辑
                        Expression.LessThanOrEqual(loopIndex, Expression.Constant(10)),
                        // 判断逻辑通过的代码
                        Expression.Block(
                            Expression.Call(
                                null,
                                typeof(System.Console).GetMethod("WriteLine", new Type[] { typeof(string) }),
                                Expression.Constant("Hello")),
                            Expression.PostIncrementAssign(loopIndex)),
                        // 判断不通过的代码
                        Expression.Break(labelBreak)
                    ), labelBreak));

            // 将我们上面的代码块表达式
            Expression<Action> lambdaExpression = Expression.Lambda<Action>(block);
            lambdaExpression.Compile().Invoke();
        }

        /// <summary>
        /// loop
        /// </summary>
        public void CreateLoop()
        {
            // 创建 loop表达式体来包含我们想要执行的代码
            LoopExpression loop = Expression.Loop(Expression.Call(
                null,
                typeof(System.Console).GetMethod("WriteLine", new Type[] { typeof(string) }) ?? throw new InvalidOperationException(),
                Expression.Constant("Hello"))
            );

            // 创建一个代码块表达式包含我们上面创建的loop表达式
            BlockExpression block = Expression.Block(loop);

            // 将我们上面的代码块表达式
            Expression<Action> lambdaExpression = Expression.Lambda<Action>(block);
            lambdaExpression.Compile().Invoke();
        }
    }
}
