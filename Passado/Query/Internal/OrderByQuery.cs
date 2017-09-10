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
        public ImmutableArray<(PropertyInfo Property, SortOrder Order)> Columns { get; protected set; }
    }

    public class OrderByQuery<TResult>
    : OrderByQueryBase
    , Select.IOrderByQuery<TResult>
    {
        public OrderByQuery(QueryBase innerBuilder, LambdaExpression selector)
        {
            InnerQuery = innerBuilder;
            Columns = ExpressionHelpers.ParseOrderedMultiPropertySelector(selector);
        }
    }
}
