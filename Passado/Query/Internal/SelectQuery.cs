using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Reflection;

using Passado.Model;
using Passado.Model.Table;
using Passado.Error;

namespace Passado.Query.Internal
{
    public abstract class SelectQueryBase : QueryBase
    {
        public ImmutableArray<PropertyModel> Columns { get; protected set; }
        public LambdaExpression Selector { get; protected set; }
    }

    public class SelectQuery<TResult>
        : SelectQueryBase
        , Select.ISelectQuery<TResult>
    {
        GroupByQueryBase GetGroupByQuery(QueryBase query)
        {
            if (query == null)
                return null;
            else if (query is GroupByQueryBase groupByQuery)
                return groupByQuery;
            else
                return GetGroupByQuery(query.InnerQuery);
        }

        public SelectQuery(QueryBase innerQuery, LambdaExpression selector)
        {
            InnerQuery = innerQuery;
            Selector = selector;

            Columns = (selector.Body as NewExpression).Members
                                                      .Select(m => new PropertyModel(m.Name, (m as PropertyInfo).PropertyType))
                                                      .ToImmutableArray();

            var groupByQuery = GetGroupByQuery(innerQuery);

            if (groupByQuery != null)
            {
                // All group by keys must be present in the select statement
                var groupByNames = new HashSet<string>(groupByQuery.KeyColumns.Select(k => k.Property.Name));
                
                foreach (var expression in (selector.Body as NewExpression).Arguments)
                {
                    if (expression is MemberExpression keyProperty &&
                        keyProperty.Member is PropertyInfo &&
                        keyProperty.Expression is MemberExpression keysProperty &&
                        keysProperty.Member is PropertyInfo &&
                        keysProperty.Expression.NodeType == ExpressionType.Parameter &&
                        keysProperty.Member.Name == nameof(IGroupedRow<object, object>.Keys))
                    {
                        groupByNames.Remove(keyProperty.Member.Name);   
                    }
                }

                if (groupByNames.Count > 0)
                {
                    throw QueryBuilderError.SelectGroupByKeyNotPresentInSelect().AsException();
                }
            }
        }
    }
}
