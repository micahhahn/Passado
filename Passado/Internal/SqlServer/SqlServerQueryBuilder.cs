using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Passado.Query;
using Passado.Query.Internal;

namespace Passado.Internal.SqlServer
{
    public class SqlServerQueryBuilder<TDatabase> : QueryBuilderBase, IQueryBuilder<TDatabase>
    {
        public SqlServerQueryBuilder()
            : base(typeof(TDatabase))
        {

        }

        public override IQuery Build(QueryBase query)
        {
            throw new NotImplementedException();
        }

        public override IQuery<TResult> Build<TResult>(QueryBase query)
        {
            var q = ParseQuery(query);

            throw new NotImplementedException();
        }

        public string ParseQuery(QueryBase query)
        {
            if (query is AsQueryBase asQuery)
            {
                return ParseFromOrJoinQuery(query.InnerQuery, asQuery.Names);
            }
            else if (query is JoinQueryBase || query is FromQueryBase)
            {
                return ParseFromOrJoinQuery(query.InnerQuery, null);
            }
            else if (query is WhereQueryBase whereQuery)
            {
                return $"{ParseQuery(query.InnerQuery)}\nWHERE {{{whereQuery.Condition.ToString()}}}";
            }
            else if (query is GroupByQueryBase groupByQuery)
            {
                return $"{ParseQuery(query.InnerQuery)}\nGROUP BY {string.Join(", ", groupByQuery.KeyColumns.Select(k => $"{{{k.Expression.ToString()}}}"))}";
            }
            else if (query is HavingQueryBase havingQuery)
            {
                return $"{ParseQuery(query.InnerQuery)}\nHAVING {{{havingQuery.Condition.ToString()}}}";
            }
            else if (query is SelectQueryBase selectQuery)
            {
                return $"SELECT {{{selectQuery.Selector.ToString()}}}\n{ParseQuery(query.InnerQuery)}";
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
        
        public string ParseFromOrJoinQuery(QueryBase query, ImmutableArray<(string DefaultName, string AsName)>? names)
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

                return $"{ParseFromOrJoinQuery(query.InnerQuery, names)}\n{joinName} {joinQuery.Model.Name} AS {GetName(joinQuery.DefaultName)} ON {{{joinQuery.Condition.ToString()}}}";
            }
            else if (query is FromQueryBase fromQuery)
            {
                return $"FROM {fromQuery.Model.Name} AS {GetName(fromQuery.Name)}";
            }

            throw new NotImplementedException();
        }
    }
}
