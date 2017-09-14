using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Internal
{
    public abstract class WhereQueryBase : QueryBase
    {
        public WhereQueryBase(QueryBase innerQuery, LambdaExpression condition)
        {
            InnerQuery = innerQuery;
            Condition = condition;
        }

        public LambdaExpression Condition { get; }
    }

    public class WhereQuery<TRow, TTable1>
        : WhereQueryBase
        , Select.IWhereQuery<TRow>
        , Update.IWhereQuery<TRow, TTable1>
        , Delete.IWhereQuery<TRow>
    {
        public WhereQuery(QueryBase innerQuery, LambdaExpression expression)
            : base(innerQuery, expression)
        { }
    }
}
