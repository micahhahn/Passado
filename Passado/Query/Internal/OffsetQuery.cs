using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Internal
{
    public abstract class OffsetQueryBase : QueryBase
    {
        public OffsetQueryBase(QueryBase innerQuery, Expression<Func<long>> offset)
        {
            InnerQuery = innerQuery;
            Offset = offset.Body;
        }

        public Expression Offset { get; }
    }

    public class OffsetQuery<TResult>
        : OffsetQueryBase
        , Select.IOffsetQuery<TResult>
    {
        public OffsetQuery(QueryBase innerQuery, Expression<Func<long>> offset)
            : base(innerQuery, offset)
        { }
    }
}
