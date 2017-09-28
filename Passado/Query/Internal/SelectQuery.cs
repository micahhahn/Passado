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
        public SelectQueryBase(QueryBase innerQuery, LambdaExpression selector)
        {
            InnerQuery = innerQuery;
            Selector = selector;
            Columns = GetColumns(selector);

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

        static GroupByQueryBase GetGroupByQuery(QueryBase query)
        {
            if (query == null)
                return null;
            else if (query is GroupByQueryBase groupByQuery)
                return groupByQuery;
            else
                return GetGroupByQuery(query.InnerQuery);
        }

        public static ImmutableArray<PropertyModel> GetColumns(LambdaExpression selector)
        {
            if (selector.Body is NewExpression newExpression)
            {
                return newExpression.Members
                                    .Select(m => new PropertyModel(m.Name, (m as PropertyInfo).PropertyType))
                                    .ToImmutableArray();
            }
            else if (selector.Body is MemberInitExpression memberInitExpression)
            {
                return memberInitExpression.Bindings
                                           .Select(b => new PropertyModel(b.Member.Name, (b as MemberAssignment).Expression.Type))
                                           .ToImmutableArray();
            }

            throw new NotImplementedException();
        }

        public ImmutableArray<PropertyModel> Columns { get; }
        public LambdaExpression Selector { get; }
    }

    public class SelectQuery<TResult>
        : SelectQueryBase
        , Select.ISelectQuery<TResult>
    {
        public SelectQuery(QueryBase innerQuery, LambdaExpression selector)
            : base(innerQuery, selector)
        { }
    }
}
