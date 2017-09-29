using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Internal
{
    public abstract class LimitQueryBase : QueryBase
    {
        public LimitQueryBase(QueryBase innerQuery, long limit)
        {
            InnerQuery = innerQuery;
            Limit = limit;
        }

        public long Limit { get; }
    }

    public class LimitQuery<TResult>
        : LimitQueryBase
        , Select.ILimitQuery<TResult>
    {
        public LimitQuery(QueryBase innerQuery, long limit)
            : base(innerQuery, limit)
        { }
    }
}
