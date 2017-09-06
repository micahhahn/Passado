using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Internal
{
    public abstract class HavingQueryBase : QueryBase
    {
        public Expression HavingCondition { get; protected set; }
    }

    public class HavingQuery<TGroupedRow>
        : HavingQueryBase
        , Select.IHavingQuery<TGroupedRow>
    {
        public HavingQuery(QueryBase innerQuery, Expression condition)
        {
            InnerQuery = innerQuery;
            HavingCondition = condition;
        }
    }
}
