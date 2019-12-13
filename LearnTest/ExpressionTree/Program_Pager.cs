#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program_Pager.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program_Pager 
* 创 建 者：Administrator 
* 创建时间：2019/12/13 15:09:05 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Learn.Console.ExpressionTree
{

    public class PageRequest
    {
        /// <summary>
        /// 每页条数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 当前页数
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortBy { get; set; }

        /// <summary>
        /// 排序方式（desc、asc）
        /// </summary>
        public string Direction { get; set; }

        /// <summary>
        /// 获取排序sql语句
        /// </summary>
        /// <returns></returns>
        public string GetOrderBySql()
        {
            if (string.IsNullOrWhiteSpace(SortBy))
            {
                return "";
            }
            var resultSql = new StringBuilder(" ORDER BY ");
            string dir = Direction;
            if (string.IsNullOrWhiteSpace(dir))
            {
                dir = "ASC";
            }
            if (SortBy.Contains("&"))
            {
                resultSql.Append("").Append(string.Join(",", SortBy.Split('&').Select(e => $" {e} {dir}").ToArray()));
            }
            else
            {
                resultSql.Append(SortBy).Append("").Append(dir);//默认处理方式
            }
            return resultSql.ToString();
        }
    }

    /// <summary>
    /// 通用分页返回
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageResponse<T>
    {
        /// <summary>
        /// 总条数
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 返回
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// 当前页
        /// </summary>
        public long PageIndex { get; set; }

        /// <summary>
        /// 每页条数
        /// </summary>
        public long PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public long TotalPages { get; set; }

        /// <summary>
        /// 返回筛选集合
        /// </summary>
        public Dictionary<string, List<string>> ResultFilter = new Dictionary<string, List<string>>();

    }


    public class PFTPage
    {
        public static PageResponse<T> DataPagination<T>(IQueryable<T> source, PageRequest page) where T : class, new()
        {

            var sesultData = new PageResponse<T>();
            bool isAsc = page.Direction.ToLower() == "asc" ? true : false;
            string[] _order = page.SortBy.Split('&');
            MethodCallExpression resultExp = null;

            foreach (string item in _order)
            {
                string _orderPart = item;
                _orderPart = Regex.Replace(_orderPart, @"\s+", "");
                string[] _orderArry = _orderPart.Split(' ');
                string _orderField = _orderArry[0];
                bool sort = isAsc;
                if (_orderArry.Length == 2)
                {
                    isAsc = _orderArry[1].ToUpper() == "ASC" ? true : false;
                }

                var parameter = Expression.Parameter(typeof(T), "t");
                var property = typeof(T).GetProperty(_orderField);
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExp = Expression.Lambda(propertyAccess, parameter);
                resultExp = Expression.Call(typeof(Queryable), isAsc ? "OrderBy" : "OrderByDescending",
                    new Type[] {typeof(T), property.PropertyType}, source.Expression, Expression.Quote(orderByExp));
            }
             
            var tempData = source.Provider.CreateQuery<T>(resultExp);
            sesultData.PageIndex = page.PageIndex;
            sesultData.PageSize = page.PageSize;
            sesultData.TotalCount = tempData.Count();

            sesultData.TotalPages = sesultData.TotalCount / sesultData.PageSize;
            if (sesultData.TotalCount % sesultData.PageSize > 0)
            {
                sesultData.TotalPages += 1;
            }

            sesultData.Items = tempData.Skip(page.PageSize * (page.PageIndex - 1)).Take(page.PageSize).ToList();
            return sesultData;
        }

       
       
    }
    public class ScoreClass
    {
        public string CourseName { get; set; }
        public string StudentName { get; set; }
        public decimal Score { get; set; }
    }

    public class Program_Pager : IMain
    {
        public void Main(string[] args)
        {
            var datas = new List<ScoreClass>();
            datas.Add(new ScoreClass
            {
                CourseName = "数学",
                StudentName = "学生A",
                Score = 60
            });
            datas.Add(new ScoreClass
            {
                CourseName = "数学",
                StudentName = "学生B",
                Score = 65
            });
            datas.Add(new ScoreClass
            {
                CourseName = "数学",
                StudentName = "学生C",
                Score = 70
            });
            datas.Add(new ScoreClass
            {
                CourseName = "数学",
                StudentName = "学生D",
                Score = 75
            });
            datas.Add(new ScoreClass
            {
                CourseName = "数学",
                StudentName = "学生E",
                Score = 80
            });
            datas.Add(new ScoreClass
            {
                CourseName = "数学",
                StudentName = "学生F",
                Score = 81
            });
            datas.Add(new ScoreClass
            {
                CourseName = "数学",
                StudentName = "学生G",
                Score = 82
            });
            datas.Add(new ScoreClass
            {
                CourseName = "数学",
                StudentName = "学生H",
                Score = 83
            });
            datas.Add(new ScoreClass
            {
                CourseName = "数学",
                StudentName = "学生I",
                Score = 84
            });
            //按照Score降序排序取第一个（5条数据）
            var page = new PageRequest()
            {
                Direction = "desc",
                PageIndex = 1,
                PageSize = 5,
                SortBy = "Score"
            };
            var result = PFTPage.DataPagination(datas.AsQueryable(), page);
            System.Console.WriteLine($"分页结果：\n{string.Join("\n", result.Items.Select(e => $"{e.StudentName} {e.CourseName} {e.Score}"))}");

            System.Console.ReadLine();
        }
    }
}
