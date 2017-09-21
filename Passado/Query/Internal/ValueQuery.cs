using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Passado.Query.Internal
{
    public abstract class ValueQueryBase : QueryBase
    {
        public ValueQueryBase(QueryBase innerQuery, LambdaExpression value)
        {
            InsertQueryBase GetInsertQuery(QueryBase query)
            {
                if (query is InsertQueryBase insertQuery)
                    return insertQuery;
                else
                    return GetInsertQuery(query.InnerQuery);
            }

            InnerQuery = innerQuery;

            if (value.Body is MemberInitExpression memberInitExpression)
            {
                var insertQuery = GetInsertQuery(innerQuery);

                var bindings = memberInitExpression.Bindings
                                                   .ToDictionary(b => b.Member.Name,
                                                                 b => (b as MemberAssignment).Expression);

                // TODO: Error on into column not found in values
                // TODO: Error on values not an into column
                Values = GetInsertQuery(innerQuery).IntoColumns
                                                   .Select(c => bindings[c.Name])
                                                   .ToImmutableArray();
            }
        }

        public ImmutableArray<Expression> Values { get; }
    }

    public class ValueQuery<TIntoTable>
        : ValueQueryBase
        , Insert.IValueQuery<TIntoTable>
    {
        public ValueQuery(QueryBase innerQuery, LambdaExpression value)
            : base(innerQuery, value)
        {

        }
    }
}
