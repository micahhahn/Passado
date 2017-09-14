using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Reflection;

using Passado.Model.Table;
using Passado.Error;

namespace Passado.Query.Internal
{
    public abstract class GroupByQueryBase : QueryBase
    {
        public GroupByQueryBase(QueryBase innerQuery, LambdaExpression keys)
        {
            InnerQuery = innerQuery;

            if (keys == null)
                throw BuilderError.ArgumentNull(nameof(keys)).AsException();

            // Keys must be named 
            if (keys.Body is NewExpression newExpression)
            {
                KeyColumns = newExpression.Members.Zip(newExpression.Arguments, (l, r) => (Left: l, Right: r))
                                                  .Select(p => (p.Left as PropertyInfo, p.Right))
                                                  .ToImmutableArray();
            }
            else
            {
                throw QueryBuilderError.GroupByNotNewExpression().AsException();
            }
        }
        
        public ImmutableArray<(PropertyInfo Property, Expression Expression)> KeyColumns { get; }
    }

    internal class GroupByQuery<TGroupedRow>
        : GroupByQueryBase
        , Select.IGroupByQuery<TGroupedRow>
    {
        // Technically keys can be any expression.  And they must be named.
        internal GroupByQuery(QueryBase innerQuery, LambdaExpression keys)
            : base(innerQuery, keys)
        { }
    }
}
