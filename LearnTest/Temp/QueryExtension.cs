#region  <<版本注释>>
/* ========================================================== 
// <copyright file="QueryExtension.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：QueryExtension 
* 创 建 者：Administrator 
* 创建时间：2020/4/1 13:12:36 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Console.Temp
{
    public static class QueryExtension
    {
        public static string Where<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate)
        {
            var expression = Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod())
                .MakeGenericMethod(new Type[] { typeof(TSource) }),
                new Expression[] { source.Expression, Expression.Quote(predicate) });
            var tran = new QueryTranslator();
            return tran.Translate(expression);
        }
    }

    public static class EString
    {
        /// <summary>
        /// 将字符串转换为Int
        /// </summary>
        /// <param name="t"></param>
        /// <returns>当转换失败时返回0</returns>
        public static int ToInt(this string t,int a)
        {
            int id;
            int.TryParse(t, out id);//这里当转换失败时返回的id为0
            return id;
        }
    }

    public static class EString2
    {
        /// <summary>
        /// 将字符串转换为Int
        /// </summary>
        /// <param name="t"></param>
        /// <returns>当转换失败时返回0</returns>
        public static string ToInt(this string t,int a)
        {
            return "";
        }
    }
}
