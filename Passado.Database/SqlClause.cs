using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;
using System.Diagnostics;

namespace Passado.Database
{
    [DebuggerDisplay("Type = {Type}, Text = {Text}, ParameterCount = {Parameters.Length}")]
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
