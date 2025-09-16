using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Helpers
{
    public static class DataTableApplyRequest
    {
        public static IQueryable<T> ApplyDataTableRequest<T>(
        this IQueryable<T> query,
        DataTableRequest req,
        Dictionary<string, Expression<Func<T, object>>> columnMap)
        {
            // 🔍 Global Search
            if (!string.IsNullOrWhiteSpace(req.Search.Value))
            {
                var search = $"%{req.Search.Value}%";
                var predicate = PredicateBuilder.False<T>();


                foreach (var colExp in columnMap.Values)
                {
                    // ambil nama property dari Expression
                    var memberExpr = colExp.Body as MemberExpression
                                    ?? (colExp.Body as UnaryExpression)?.Operand as MemberExpression;

                    if (memberExpr == null) continue;

                    var propName = memberExpr.Member.Name;
                    var propType = ((PropertyInfo)memberExpr.Member).PropertyType;

                    // string
                    if (propType == typeof(string))
                    {
                        var pattern = $"%{search}%";
                        Expression<Func<T, bool>> likeExp =
                            p => EF.Functions.Like(EF.Property<string>(p!, propName), pattern);

                        predicate = predicate.Or(likeExp);
                    }
                    else
                    {
                        var pattern = $"%{search}%";
                        Expression<Func<T, bool>> likeExp =
                            p => EF.Functions.Like(EF.Property<object>(p!, propName).ToString(), pattern);

                        predicate = predicate.Or(likeExp);
                    }

                    // fallback (tidak dipakai)
                }
                query = query.Where(predicate);
            }
            /* if (!string.IsNullOrWhiteSpace(req.Search.Value))
                {
                    var search = $"%{req.Search.Value}%";
                    var predicate = PredicateBuilder.False<T>();

                    foreach (var colExp in columnMap.Values)
                    {
                        // ambil nama property dari Expression
                        var memberExpr = colExp.Body as MemberExpression
                                        ?? ((UnaryExpression)colExp.Body).Operand as MemberExpression;

                        if (memberExpr == null) continue;

                        var propName = memberExpr.Member.Name;

                        Expression<Func<T, bool>> likeExp = p =>
                            EF.Functions.Like(
                                EF.Property<object>(p!, propName).ToString(),
                                search
                            );

                        predicate = predicate.Or(likeExp);
                        // buat parameter baru
                        // var param = Expression.Parameter(typeof(Product), "p");
                    }

                    query = query.Where(predicate);
                } */

            // 🔍 Per-column Search
            foreach (var col in req.Columns.Where(c => c.Searchable && !string.IsNullOrWhiteSpace(c.Search.Value)))
            {
                if (columnMap.TryGetValue(col.Data, out var colExp))
                {
                    var search = $"%{col.Search.Value}%";

                    // ambil nama property dari Expression
                    var memberExpr = colExp.Body as MemberExpression
                                    ?? ((UnaryExpression)colExp.Body).Operand as MemberExpression;

                    if (memberExpr == null) continue;

                    var propName = memberExpr.Member.Name;
                    var propType = ((PropertyInfo)memberExpr.Member).PropertyType;

                    if (propType == typeof(string))
                    {
                        query = query.Where(p =>
                        EF.Functions.Like(
                            EF.Property<string>(p!, propName),
                            search
                        )
                    );
                    }
                    else
                    {
                        query = query.Where(p =>
                           EF.Functions.Like(
                               EF.Property<object>(p!, propName).ToString(),
                               search
                           )
                       );
                    }
                }
            }

            // 🔽 Sorting
            if (req.Order.Count > 0)
            {
                var firstOrder = req.Order.First();
                var sortCol = req.Columns[firstOrder.Column].Data;
                var dir = firstOrder.Dir;

                if (columnMap.TryGetValue(sortCol, out var sortExp))
                {
                    query = dir == "asc" ? query.OrderBy(sortExp) : query.OrderByDescending(sortExp);
                }
            }

            return query;
        }
    }
}