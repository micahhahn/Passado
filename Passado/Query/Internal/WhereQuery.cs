using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Internal
{
    public class WhereQuery<TRow, TTable1>
        : QueryBase
        , Select.IWhereQuery<TRow>
        , Update.IWhereQuery<TRow, TTable1>
        , Delete.IWhereQuery<TRow>
    {
        public WhereQuery(QueryBase innerQuery, Expression expression)
        {
            InnerQuery = innerQuery;
            Condition = expression;
        }

        public Expression Condition { get; }
    }
}
