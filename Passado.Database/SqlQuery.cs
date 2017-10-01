using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Passado.Database
{
    public class SqlQuery
    {
        public SqlQuery(string queryText, ImmutableArray<MemberExpression> parameters)
        {
            QueryText = queryText;
            Parameters = parameters;
        }

        public string QueryText { get; }
        public ImmutableArray<MemberExpression> Parameters { get; }
    }
}
