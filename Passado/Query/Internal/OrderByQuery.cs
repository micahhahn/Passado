using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Reflection;

using Passado.Model.Table;
using Passado.Model;

namespace Passado.Query.Internal
{
    public abstract class OrderByQueryBase : QueryBase
    {
        public OrderByQueryBase(QueryBase innerQuery, LambdaExpression selector)
        {
            InnerQuery = innerQuery;
            Columns = ExpressionHelpers.ParseOrderedMultiPropertySelector(selector);
        }

        public ImmutableArray<(PropertyInfo Property, SortOrder Order)> Columns { get; }
    }

    public class OrderByQuery<TResult>
    : OrderByQueryBase
    , Select.IOrderByQuery<TResult>
    {
        public OrderByQuery(QueryBase innerQuery, LambdaExpression selector)
            : base(innerQuery, selector)
        { }
    }
}
