using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;
using System.Data;

using Passado.Query;
using Passado.Query.Internal;

namespace Passado.Database
{
    using VariableDictionary = ImmutableDictionary<(Type ClosureType, string MemberName), (string VariableName, Func<object> ValueGetter)>;

    public abstract class DatabaseQueryBuilder<TDatabase> : QueryBuilderBase, IQueryBuilder<TDatabase>
    {
        public DatabaseQueryBuilder()
            : base(typeof(TDatabase))
        { }

        /// <summary>
        /// Escapes an identifier.
        /// </summary>
        /// <param name="identifier">The identifier to escape.</param>
        /// <returns>The escaped identifier.</returns>
        /// <remarks>
        ///     <para>
        ///           The standard SQL method to escape an identifier is using double quotes.
        ///           Specific database vendors such as [SqlServer] and `MySql` may override this.
        ///     </para>
        /// </remarks>
        public virtual string EscapeIdentifier(string identifier)
        {
            return $"\"{identifier.Replace("\"", "\"\"")}\"";
        }
        
        public SqlQuery ParseQuery(QueryBase query)
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
                var innerQuery = ParseQuery(query.InnerQuery);
                var parsedWhereQuery = ParseExpression(whereQuery.Condition.Body, query, innerQuery.Parameters);
                return new SqlQuery($"{innerQuery.QueryText}\nWHERE {parsedWhereQuery.QueryText}", parsedWhereQuery.Parameters);
            }
            else if (query is GroupByQueryBase groupByQuery)
            {
                var innerQuery = ParseQuery(query.InnerQuery);
                var keysQuery = groupByQuery.KeyColumns.Aggregate(new SqlQuery("", innerQuery.Parameters), (q, c) =>
                {
                    var columnExpression = ParseExpression(c.Expression, query, q.Parameters);
                    return new SqlQuery($"{q.QueryText}, {columnExpression.QueryText}", columnExpression.Parameters);
                });

                return new SqlQuery($"{innerQuery.QueryText}\nGROUP BY {keysQuery.QueryText.Substring(2)}", keysQuery.Parameters);
            }
            else if (query is HavingQueryBase havingQuery)
            {
                var innerQuery = ParseQuery(query.InnerQuery);
                var parsedHavingQuery = ParseExpression(havingQuery.Condition.Body, query, innerQuery.Parameters);
                return new SqlQuery($"{innerQuery.QueryText}\nHAVING {parsedHavingQuery.QueryText}", parsedHavingQuery.Parameters);
            }
            else if (query is SelectQueryBase || query is ScalarSelectQueryBase)
            {
                var selector = (query as SelectQueryBase)?.Selector ?? (query as ScalarSelectQueryBase).Selector;

                IEnumerable<(string Name, Expression Expression)> args = null;
                if (selector.Body is NewExpression newExpression)
                {
                    args = newExpression.Constructor
                                        .GetParameters()
                                        .Zip(newExpression.Arguments, (p, e) => (p.Name, e));
                }
                else if (selector.Body is MemberInitExpression memberInitExpression)
                {
                    args = memberInitExpression.NewExpression
                                               .Constructor
                                               .GetParameters()
                                               .Zip(memberInitExpression.NewExpression.Arguments, (p, e) => (p.Name, e))
                                               .Concat(memberInitExpression.Bindings
                                                                           .Select(b => (b.Member.Name, (b as MemberAssignment).Expression)));
                }
                else if (selector.Body is MethodCallExpression methodCallExpression)
                {
                    args = methodCallExpression.Method
                                               .GetParameters()
                                               .Zip(methodCallExpression.Arguments, (p, e) => (p.Name, e));
                }
                
                var innerQuery = query.InnerQuery == null ? new SqlQuery(null, ImmutableArray.Create<MemberExpression>()) : ParseQuery(query.InnerQuery);
                var separator = ",\n       ";

                var selectQueries = args.Aggregate(new SqlQuery("", innerQuery.Parameters), (q, s) =>
                {
                    var selectExpression = ParseExpression(s.Expression, query, q.Parameters);
                    return new SqlQuery($"{q.QueryText}{separator}{selectExpression.QueryText} AS {s.Name}", selectExpression.Parameters);
                });

                return new SqlQuery($"SELECT {selectQueries.QueryText.Substring(separator.Length)}{(innerQuery.QueryText != null ? $"\n{innerQuery.QueryText}" : "")}", selectQueries.Parameters);
            }
            else if (query is OrderByQueryBase orderByQuery)
            {
                var innerQuery = ParseQuery(query.InnerQuery);
                return new SqlQuery($"{innerQuery.QueryText}\nORDER BY {string.Join(", ", orderByQuery.Columns.Select(c => $"{c.Property.Name} {(c.Order == Model.SortOrder.Ascending ? "ASC" : "DESC")}"))}", innerQuery.Parameters);
            }
            else if (query is OffsetQueryBase offsetQuery)
            {
                var innerQuery = ParseQuery(query.InnerQuery);
                return new SqlQuery($"{innerQuery.QueryText}\nOFFSET {offsetQuery.Offset}", innerQuery.Parameters);
            }
            else if (query is LimitQueryBase limitQuery)
            {
                var innerQuery = ParseQuery(query.InnerQuery);
                return new SqlQuery($"{innerQuery.QueryText}\nLIMIT {limitQuery.Limit}", innerQuery.Parameters);
            }
            else if (query is InsertQueryBase insertQuery)
            {
                return new SqlQuery($"INSERT INTO {insertQuery.Model.Name} ({string.Join(", ", insertQuery.IntoColumns.Select(c => c.Name))})", ImmutableArray.Create<MemberExpression>());
            }
            else if (query is ValueQueryBase valueQuery)
            {
                var innerQuery = ParseQuery(query.InnerQuery);
                var valuesQuery = valueQuery.Values.Aggregate(new SqlQuery("", innerQuery.Parameters), (q, c) =>
                {
                    var columnExpression = ParseExpression(c, query, q.Parameters);
                    return new SqlQuery($"{q.QueryText}, {columnExpression.QueryText}", columnExpression.Parameters);
                });
                
                return new SqlQuery($"{innerQuery.QueryText}\nVALUES ({valuesQuery.QueryText.Substring(2)})", valuesQuery.Parameters);
            }
            else
            {
                return ParseQuery(query.InnerQuery);
            }
        }

        SqlQuery ParseFromOrJoinQuery(QueryBase query, ImmutableArray<(string DefaultName, string AsName)>? names)
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

                var innerQuery = ParseFromOrJoinQuery(query.InnerQuery, names);
                var onExpression = ParseExpression(joinQuery.Condition.Body, query, innerQuery.Parameters);
                return new SqlQuery($"{innerQuery.QueryText}\n{joinName} {joinQuery.Model.Name} AS {GetName(joinQuery.DefaultName)} ON {onExpression.QueryText}", onExpression.Parameters);
            }
            else if (query is FromQueryBase fromQuery)
            {
                return new SqlQuery($"FROM {fromQuery.Model.Name} AS {GetName(fromQuery.Name)}", ImmutableArray.Create<MemberExpression>());
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

        SqlQuery ParseExpression(Expression expression, QueryBase context, ImmutableArray<MemberExpression> parameters)
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

                    var leftQuery = ParseExpression(binaryExpression.Left, context, parameters);
                    var rightQuery = ParseExpression(binaryExpression.Right, context, leftQuery.Parameters);
                    return new SqlQuery($"({leftQuery.QueryText}) {op} ({rightQuery.QueryText})", rightQuery.Parameters);
                case ConstantExpression constantExpression:
                    if (constantExpression.Type == typeof(string))
                        return new SqlQuery($"'{constantExpression.Value}'", parameters);
                    else if (constantExpression.Type == typeof(int))
                        return new SqlQuery($"{constantExpression.Value}", parameters);
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
                            
                            return ParseExpression(groupByQuery.KeyColumns.First(k => k.Property.Name == memberExpression.Member.Name).Expression, groupByQuery, parameters);
                        }
                        else
                        {
                            return new SqlQuery($"{innerMemberExpression.Member.Name}.{memberExpression.Member.Name}", parameters);
                        }
                    }
                    else if (memberExpression.NodeType == ExpressionType.MemberAccess &&
                             memberExpression.Expression is ConstantExpression constantExpression)
                    {
                        // Closures
                        return new SqlQuery($"{{{parameters.Length}}}", parameters.Add(memberExpression));
                    }
                    break;
                case MethodCallExpression methodCallExpression:
                    if (methodCallExpression.Object.NodeType == ExpressionType.Parameter)
                    {
                        var name = methodCallExpression.Method.Name;

                        if (name == nameof(IAggregatable<object>.Count) && methodCallExpression.Arguments.Count == 0)
                            return new SqlQuery("COUNT(*)", parameters);
                        else
                        {
                            var function = name == nameof(IAggregatable<object>.Count) ? "COUNT(" :
                                           name == nameof(IAggregatable<object>.CountDistinct) ? "COUNT(DISTINCT " :
                                           name == nameof(IAggregatable<object>.Min) ? "MIN(" :
                                           name == nameof(IAggregatable<object>.Max) ? "MAX(" :
                                           throw new NotImplementedException();

                            var functionQuery = ParseExpression((methodCallExpression.Arguments[0] as LambdaExpression).Body, context, parameters);
                            return new SqlQuery($"{function}{functionQuery.QueryText})", functionQuery.Parameters);
                        }
                    }
                    break;
                default:
                    break;
            }

            return new SqlQuery($"{{{expression.ToString()}}}", parameters);
        }
    }
}
