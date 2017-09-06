using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Passado.Model.Table;

namespace Passado.Query.Internal
{
    public abstract class OrderByQueryBase : QueryBase
    {
        public ImmutableArray<SortedColumnModel> Columns { get; protected set; }
    }

    public class OrderByQuery<TResult>
    : OrderByQueryBase
    , Select.IOrderByQuery<TResult>
    {
        public OrderByQuery(QueryBase innerBuilder, LambdaExpression selector)
        {
            InnerQuery = innerBuilder;
            Columns = ExpressionHelpers.ParseOrderedMultiPropertySelector(selector).MatchColumns("asdf", (innerBuilder as SelectQueryBase).Columns);
        }
    }
}
