using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;

using Passado.Query;
using Passado.Query.Internal;

namespace Passado.Internal
{
    public abstract class DatabaseQueryBuilder<TDatabase> : QueryBuilderBase, IQueryBuilder<TDatabase>
    {
        public DatabaseQueryBuilder()
            : base(typeof(TDatabase))
        { }

        public abstract string EscapeName(string name);

        public string ParseQuery(QueryBase query)
        {
            if (query is AsQueryBase asQuery)
            {
                return ParseFromOrJoinQuery(query.InnerQuery, asQuery.Names);
            }
            else if (query is JoinQueryBase || query is FromQueryBase)
            {
                return ParseFromOrJoinQuery(query, null);
            }
            else if (query is WhereQueryBase whereQuery)
            {
                return $"{ParseQuery(query.InnerQuery)}\nWHERE {ParseExpression(whereQuery.Condition.Body, query)}";
            }
            else if (query is GroupByQueryBase groupByQuery)
            {
                return $"{ParseQuery(query.InnerQuery)}\nGROUP BY {string.Join(", ", groupByQuery.KeyColumns.Select(k => $"{ParseExpression(k.Expression, query)}"))}";
            }
            else if (query is HavingQueryBase havingQuery)
            {
                return $"{ParseQuery(query.InnerQuery)}\nHAVING {ParseExpression(havingQuery.Condition.Body, query)}";
            }
            else if (query is SelectQueryBase selectQuery)
            {
                if (selectQuery.Selector.Body is NewExpression newExpression)
                {
                    var args = newExpression.Arguments.Zip(newExpression.Members, (a, m) => (Argument: a, Member: m))
                                                      .Select(p => $"{ParseExpression(p.Argument, query)} AS {p.Member.Name}");
                    return $"SELECT {string.Join(",\n       ", args)}\n{ParseQuery(query.InnerQuery)}";
                }

                throw new NotImplementedException();
            }
            else if (query is OrderByQueryBase orderByQuery)
            {
                return $"{ParseQuery(query.InnerQuery)}\nORDER BY {string.Join(", ", orderByQuery.Columns.Select(c => $"{c.Property.Name} {(c.Order == Model.SortOrder.Ascending ? "ASC" : "DESC")}"))}";
            }
            else
            {
                return ParseQuery(query.InnerQuery);
            }
        }

        string ParseFromOrJoinQuery(QueryBase query, ImmutableArray<(string DefaultName, string AsName)>? names)
        {
            string GetName(string defaultName)
            {
                if (names == null)
                    return defaultName;
                else
                    return names?.First(n => n.DefaultName == defaultName).AsName;
            }

            if (query is JoinQueryBase joinQuery)
            {
                var joinName = joinQuery.JoinType == JoinType.Inner ? "JOIN" :
                               joinQuery.JoinType == JoinType.Left ? "LEFT JOIN" :
                               joinQuery.JoinType == JoinType.Right ? "RIGHT JOIN" :
                               joinQuery.JoinType == JoinType.Outer ? "OUTER JOIN" :
                               joinQuery.JoinType == JoinType.Cross ? "CROSS JOIN" :
                               throw new NotImplementedException();

                return $"{ParseFromOrJoinQuery(query.InnerQuery, names)}\n{joinName} {joinQuery.Model.Name} AS {GetName(joinQuery.DefaultName)} ON {ParseExpression(joinQuery.Condition.Body, query)}";
            }
            else if (query is FromQueryBase fromQuery)
            {
                return $"FROM {fromQuery.Model.Name} AS {GetName(fromQuery.Name)}";
            }

            throw new NotImplementedException();
        }

        GroupByQueryBase GetGroupByQuery(QueryBase query)
        {
            if (query is GroupByQueryBase groupByQuery)
                return groupByQuery;
            else
                return GetGroupByQuery(query.InnerQuery);
        }

        string ParseExpression(Expression expression, QueryBase context)
        {
            switch (expression)
            {
                case BinaryExpression binaryExpression:
                    var op = binaryExpression.NodeType == ExpressionType.AndAlso ? "AND" :
                             binaryExpression.NodeType == ExpressionType.OrElse ? "OR" :
                             binaryExpression.NodeType == ExpressionType.Equal ? "=" :
                             binaryExpression.NodeType == ExpressionType.NotEqual ? "!=" :
                             binaryExpression.NodeType == ExpressionType.LessThan ? "<" :
                             binaryExpression.NodeType == ExpressionType.LessThanOrEqual ? "<=" :
                             binaryExpression.NodeType == ExpressionType.GreaterThan ? ">" :
                             binaryExpression.NodeType == ExpressionType.GreaterThanOrEqual ? ">=" :
                             binaryExpression.NodeType == ExpressionType.Add ? "+" :
                             throw new NotImplementedException();

                    return $"({ParseExpression(binaryExpression.Left, context)}) {op} ({ParseExpression(binaryExpression.Right, context)})";
                case ConstantExpression constantExpression:
                    if (constantExpression.Type == typeof(string))
                        return $"'{constantExpression.Value}'";
                    else if (constantExpression.Type == typeof(int))
                        return $"{constantExpression.Value}";
                    else
                        break;
                case MemberExpression memberExpression:
                    if (memberExpression.NodeType == ExpressionType.MemberAccess &&
                        memberExpression.Expression is MemberExpression innerMemberExpression &&
                        innerMemberExpression.Expression.NodeType == ExpressionType.Parameter)
                    {
                        if (innerMemberExpression.Expression.Type.Name.StartsWith(nameof(IGroupedRow<object, object>)) &&
                            innerMemberExpression.Member.Name == nameof(IGroupedRow<object, object>.Keys))
                        {
                            var groupByQuery = GetGroupByQuery(context);

                            return ParseExpression(groupByQuery.KeyColumns.First(k => k.Property.Name == memberExpression.Member.Name).Expression, groupByQuery);
                        }
                        else
                        {
                            return $"{innerMemberExpression.Member.Name}.{memberExpression.Member.Name}";
                        }
                    }
                    break;
                case MethodCallExpression methodCallExpression:
                    if (methodCallExpression.Object.NodeType == ExpressionType.Parameter)
                    {
                        var name = methodCallExpression.Method.Name;

                        if (name == nameof(IAggregatable<object>.Count) && methodCallExpression.Arguments.Count == 0)
                            return "COUNT(*)";
                        else
                        {
                            var function = name == nameof(IAggregatable<object>.Count) ? "COUNT(" :
                                           name == nameof(IAggregatable<object>.CountDistinct) ? "COUNT(DISTINCT " :
                                           name == nameof(IAggregatable<object>.Min) ? "MIN(" :
                                           name == nameof(IAggregatable<object>.Max) ? "MAX(" :
                                           throw new NotImplementedException();

                            return $"{function}{ParseExpression((methodCallExpression.Arguments[0] as LambdaExpression).Body, context)})";
                        }
                    }
                    break;
                default:
                    break;
            }

            return $"{{{expression.ToString()}}}";
        }
    }
}
