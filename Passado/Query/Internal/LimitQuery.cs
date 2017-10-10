using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Internal
{
    public abstract class LimitQueryBase : QueryBase
    {
        public LimitQueryBase(QueryBase innerQuery, Expression<Func<long>> limit)
        {
            InnerQuery = innerQuery;
            Limit = limit.Body;
        }

        public Expression Limit { get; }
    }

    public class LimitQuery<TResult>
        : LimitQueryBase
        , Select.ILimitQuery<TResult>
    {
        public LimitQuery(QueryBase innerQuery, Expression<Func<long>> limit)
            : base(innerQuery, limit)
        { }
    }
}
