using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Internal
{
    public abstract class HavingQueryBase : QueryBase
    {
        public HavingQueryBase(QueryBase innerQuery, LambdaExpression condition)
        {
            InnerQuery = innerQuery;
            Condition = condition;
        }

        public Expression Condition { get; }
    }

    public class HavingQuery<TGroupedRow>
        : HavingQueryBase
        , Select.IHavingQuery<TGroupedRow>
    {
        public HavingQuery(QueryBase innerQuery, LambdaExpression condition)
            : base(innerQuery, condition)
        { }
    }
}
