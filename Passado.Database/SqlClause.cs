using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;

namespace Passado.Database
{
    public class SqlClause
    {
        public SqlClause(ClauseType type, string text, IEnumerable<MemberExpression> parameters)
        {
            Type = type;
            Text = text;
            Parameters = parameters.ToImmutableArray();
        }

        public ClauseType Type { get; }
        public string Text { get; }
        public ImmutableArray<MemberExpression> Parameters { get; set; }
    }
}
