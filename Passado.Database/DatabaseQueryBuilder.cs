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
        
        public ImmutableArray<SqlClause> ParseQuery(QueryBase query)
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
                var parsedWhereExpression = ParseExpression(whereQuery.Condition.Body, ClauseType.Where, query);
                return innerQuery.Add(new SqlClause(ClauseType.Where, $"WHERE {parsedWhereExpression.Text}", parsedWhereExpression.Parameters));
            }
            else if (query is GroupByQueryBase groupByQuery)
            {
                var innerQuery = ParseQuery(query.InnerQuery);
                (var text, var parameters) = groupByQuery.KeyColumns.Aggregate((Text: "", Parameters: ImmutableArray.Create<MemberExpression>()), (q, c) =>
                {
                    var columnExpression = ParseExpression(c.Expression, ClauseType.GroupBy, query);
                    return ($"{q.Text}, {columnExpression.Text}", columnExpression.Parameters);
                });

                return innerQuery.Add(new SqlClause(ClauseType.GroupBy, text, parameters));
            }
            else if (query is HavingQueryBase havingQuery)
            {
                var innerQuery = ParseQuery(query.InnerQuery);
                var parsedHavingQuery = ParseExpression(havingQuery.Condition.Body, ClauseType.Having, query);
                return innerQuery.Add(new SqlClause(ClauseType.Having, $"HAVING {parsedHavingQuery.Text}", parsedHavingQuery.Parameters));
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
                
                var innerQuery = query.InnerQuery == null ? ImmutableArray.Create<SqlClause>() : ParseQuery(query.InnerQuery);

                var selectQueries = args.Aggregate((Lines: ImmutableArray.Create<string>(), Parameters: ImmutableArray.Create<MemberExpression>()), (q, s) =>
                {
                    var selectExpression = ParseExpression(s.Expression, ClauseType.Select, query);
                    var selectText = string.Format(selectExpression.Text, selectExpression.Parameters.Select((m, i) => $"{{{i + q.Parameters.Length}}}").ToArray());
                    return (q.Lines.Add($"{selectText} AS {s.Name}"), q.Parameters.AddRange(selectExpression.Parameters));
                });
                
                // Selct clause always goes at the beginning
                return innerQuery.Insert(0, new SqlClause(ClauseType.Select, $"SELECT {string.Join(",\n       ", selectQueries.Lines)}", selectQueries.Parameters));
            }
            else if (query is OrderByQueryBase orderByQuery)
            {
                var innerQuery = ParseQuery(query.InnerQuery);
                var columns = orderByQuery.Columns.Select(c => $"{c.Property.Name} {(c.Order == Model.SortOrder.Ascending ? "ASC" : "DESC")}");
                return innerQuery.Add(new SqlClause(ClauseType.OrderBy, $"ORDER BY {string.Join(", ", columns)}", ImmutableArray.Create<MemberExpression>()));
            }
            else if (query is OffsetQueryBase offsetQuery)
            {
                var innerQuery = ParseQuery(query.InnerQuery);
                var newClause = offsetQuery.Offset is MemberExpression memberExpression ? new SqlClause(ClauseType.Offset, "OFFSET {0}", ImmutableArray.Create(memberExpression)) :
                                offsetQuery.Offset is ConstantExpression constantExpression ? new SqlClause(ClauseType.Offset, $"OFFSET {constantExpression.Value}", ImmutableArray.Create<MemberExpression>()) :
                                throw new NotImplementedException();

                return innerQuery.Add(newClause);
            }
            else if (query is LimitQueryBase limitQuery)
            {
                var innerQuery = ParseQuery(query.InnerQuery);

                var newClause = limitQuery.Limit is MemberExpression memberExpression ? new SqlClause(ClauseType.Limit, "LIMIT {0}", ImmutableArray.Create(memberExpression)) :
                                limitQuery.Limit is ConstantExpression constantExpression ? new SqlClause(ClauseType.Limit, $"LIMIT {constantExpression.Value}", ImmutableArray.Create<MemberExpression>()) :
                                throw new NotImplementedException();

                if (query.InnerQuery is OffsetQueryBase)
                {
                    // The logical order is to do offset and then limit, but most sql databases have it in reverse order
                    var offsetClause = innerQuery.Last();
                    return innerQuery.RemoveAt(innerQuery.Length - 1)
                                     .Add(newClause)
                                     .Add(offsetClause);
                }

                throw new NotImplementedException();
            }
            else if (query is InsertQueryBase insertQuery)
            {
                var columns = string.Join(", ", insertQuery.IntoColumns.Select(c => c.Name));
                return ImmutableArray.Create(new SqlClause(ClauseType.Insert, $"INSERT INTO {insertQuery.Model.Name} ({columns})", ImmutableArray.Create<MemberExpression>()));
            }
            else if (query is ValueQueryBase valueQuery)
            {
                var innerQuery = ParseQuery(query.InnerQuery);
                var valuesQuery = valueQuery.Values.Aggregate((Text: "", Parameters: ImmutableArray.Create<MemberExpression>()), (q, c) =>
                {
                    var columnExpression = ParseExpression(c, ClauseType.Values, query);
                    return ($"{q.Text}, {columnExpression.Text}", columnExpression.Parameters);
                });

                var prelude = query.InnerQuery is ValueQueryBase ? "     , " : "VALUES ";

                return innerQuery.Add(new SqlClause(ClauseType.Values, $"{prelude}({valuesQuery.Text.Substring(2)})", valuesQuery.Parameters));
            }
            else
            {
                return ParseQuery(query.InnerQuery);
            }
        }

        ImmutableArray<SqlClause> ParseFromOrJoinQuery(QueryBase query, ImmutableArray<(string DefaultName, string AsName)>? names)
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
                var onExpression = ParseExpression(joinQuery.Condition.Body, ClauseType.Join, query);
                return innerQuery.Add(new SqlClause(ClauseType.Join, $"{joinName} {joinQuery.Model.Name} AS {GetName(joinQuery.DefaultName)} ON {onExpression.Text}", onExpression.Parameters));
            }
            else if (query is FromQueryBase fromQuery)
            {
                return ImmutableArray.Create(new SqlClause(ClauseType.From, $"FROM {fromQuery.Model.Name} AS {GetName(fromQuery.Name)}", ImmutableArray.Create<MemberExpression>()));
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

        (string Text, ImmutableArray<MemberExpression> Parameters) ParseExpression(Expression expression, ClauseType context, QueryBase innerQuery)
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

                    var leftQuery = ParseExpression(binaryExpression.Left, context, innerQuery);
                    var rightQuery = ParseExpression(binaryExpression.Right, context, innerQuery);
                    var adjustedRightText = string.Format(rightQuery.Text, rightQuery.Parameters.Select((p, i) => $"{{{i + leftQuery.Parameters.Length}}}"));
                    return ($"({leftQuery.Text}) {op} ({adjustedRightText})", leftQuery.Parameters.AddRange(rightQuery.Parameters));

                case ConstantExpression constantExpression:
                    if (constantExpression.Type == typeof(string))
                        return ($"'{constantExpression.Value}'", ImmutableArray.Create<MemberExpression>());
                    else if (constantExpression.Type == typeof(int))
                        return ($"{constantExpression.Value}", ImmutableArray.Create<MemberExpression>());
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
                            var groupByQuery = GetGroupByQuery(innerQuery);
                            
                            return ParseExpression(groupByQuery.KeyColumns.First(k => k.Property.Name == memberExpression.Member.Name).Expression, context, innerQuery);
                        }
                        else
                        {
                            return ($"{innerMemberExpression.Member.Name}.{memberExpression.Member.Name}", ImmutableArray.Create<MemberExpression>());
                        }
                    }
                    else if (memberExpression.NodeType == ExpressionType.MemberAccess &&
                             (memberExpression.Expression is ConstantExpression constantExpression || memberExpression.Expression is null))
                    {
                        // Closures
                        return ("{0}", ImmutableArray.Create(memberExpression));
                    }
                    break;
                case MethodCallExpression methodCallExpression:
                    if (methodCallExpression.Object.NodeType == ExpressionType.Parameter)
                    {
                        var name = methodCallExpression.Method.Name;

                        if (name == nameof(IAggregatable<object>.Count) && methodCallExpression.Arguments.Count == 0)
                            return ("COUNT(*)", ImmutableArray.Create<MemberExpression>());
                        else
                        {
                            var function = name == nameof(IAggregatable<object>.Count) ? "COUNT(" :
                                           name == nameof(IAggregatable<object>.CountDistinct) ? "COUNT(DISTINCT " :
                                           name == nameof(IAggregatable<object>.Min) ? "MIN(" :
                                           name == nameof(IAggregatable<object>.Max) ? "MAX(" :
                                           throw new NotImplementedException();

                            var functionQuery = ParseExpression((methodCallExpression.Arguments[0] as LambdaExpression).Body, context, innerQuery);
                            return ($"{function}{functionQuery.Text})", functionQuery.Parameters);
                        }
                    }
                    break;
                default:
                    break;
            }

            throw new NotImplementedException();
        }
    }
}
