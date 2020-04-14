#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program 
* 创 建 者：Administrator 
* 创建时间：2019/7/2 13:13:03 
* =============================================================*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using AutoMapper;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Learn.Console.Temp
{
    public class Program : IMain
    {
        public void Main(string[] args)
        {

            //Func<int[],int,int,int> fcc = (x, y, z) => x[y] = x[y] + z;

            //ParameterExpression array = Expression.Parameter(typeof(int[]),"array");
            //ParameterExpression index = Expression.Parameter(typeof(int),"index");
            //ParameterExpression value = Expression.Parameter(typeof(int),"value");

            //var indexExpression= Expression.ArrayAccess(array,index);

            //Expression<Func<int[], int, int, int>> exFun = Expression.Lambda<Func<int[], int, int, int>>(
            //    Expression.Assign(indexExpression,Expression.Add(indexExpression,value)),
            //    array,
            //    index,
            //    value
            //    );
            //System.Console.WriteLine(indexExpression.ToString());

            //NewExpression nExpression = Expression.New(typeof(Dictionary<int,string>));


            //Expression<Func<int, int, bool>> largeSumTest =
            //    (num1, num2) => (num1 + num2) > 1000;


            //InvocationExpression invocationExpression = Expression.Invoke(
            //    largeSumTest,
            //    Expression.Constant(539),
            //    Expression.Constant(281));


            //Expression<Func<int, bool>> isOld = value => value % 2 == 1;
            //Expression<Func<int, bool>> isLagerNumber = value => value > 300;

            //InvocationExpression callLeft = Expression.Invoke(isOld,Expression.Constant(5));
            //InvocationExpression callRight = Expression.Invoke(isOld, Expression.Constant(5));
            //BinaryExpression combined = Expression.MakeBinary(ExpressionType.Or,callLeft,callRight);


            //Expression<Func<bool>> typeCombined = Expression.Lambda<Func<bool>>(combined);
            //Func<bool> complied = typeCombined.Compile();
            //bool answer = complied();

            var epp = Expression.Call(null,typeof(System.Console).GetMethod("WriteLine",new Type[]{typeof(string)}),Expression.Constant("ss"));
            System.Console.WriteLine(epp);
            List<User> myUsers = new List<User>();
            string userSql = myUsers.AsQueryable().Where(x=>x.Age>10);
            System.Console.WriteLine(userSql);
            // SELECT * FROM (SELECT * FROM User) AS T WHERE (Age>2)
            
            System.Console.ReadLine();
        }
    }

    public class User
    {
        public int Age { get; set; }
    }

    class QueryTranslator : ExpressionVisitor
    {
        private StringBuilder sb = new StringBuilder();
        internal string Translate(Expression expression)
        {
            this.sb = new StringBuilder();
            this.Visit(expression);
            return this.sb.ToString();
        }
        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(QueryExtensions) && m.Method.Name == "Where")
            {
                sb.Append("SELECT * FROM (");
                this.Visit(m.Arguments[0]);
                sb.Append(") AS T WHERE ");
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                this.Visit(lambda.Body);
                return m;
            }
            throw new NotSupportedException(string.Format("方法{0}不支持", m.Method.Name));
        }
        protected override Expression VisitBinary(BinaryExpression b)
        {
            sb.Append("(");
            this.Visit(b.Left);
            switch (b.NodeType)
            {
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;
                case ExpressionType.Or:
                    sb.Append(" OR");
                    break;
                case ExpressionType.Equal:
                    sb.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    sb.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;
                default:
                    throw new NotSupportedException(string.Format("二元运算符{ 0 }不支持", b.NodeType));
            }
            this.Visit(b.Right);
            sb.Append(")");
            return b;
        }
        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;
            if (q != null)
            {
                // 我们假设我们那个Queryable就是对应的表
                sb.Append("SELECT * FROM ");
                sb.Append(q.ElementType.Name);
            }
            else if (c.Value == null)
            {
                sb.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? 1 : 0);
                        break;
                    case TypeCode.String:
                        sb.Append("'");
                        sb.Append(c.Value);
                        sb.Append("'");
                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                    default:
                        sb.Append(c.Value);
                        break;
                }
            }
            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                sb.Append(m.Member.Name);
                return m;
            }
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }
    }
}
