using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Internal
{
    public abstract class OffsetQueryBase : QueryBase
    {
        public OffsetQueryBase(QueryBase innerQuery, long offset)
        {
            InnerQuery = innerQuery;
            Offset = offset;
        }

        public long Offset { get; }
    }

    public class OffsetQuery<TResult>
        : OffsetQueryBase
        , Select.IOffsetQuery<TResult>
    {
        public OffsetQuery(QueryBase innerQuery, long offset)
            : base(innerQuery, offset)
        { }
    }
}
