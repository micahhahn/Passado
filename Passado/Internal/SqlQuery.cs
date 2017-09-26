using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Passado.Internal
{
    using VariableDictionary = ImmutableDictionary<(Type ClosureType, string MemberName), (string VariableName, Func<object> ValueGetter)>;

    public class SqlQuery
    {
        public SqlQuery(string queryText, VariableDictionary variables)
        {
            QueryText = queryText;
            Variables = variables;
        }

        public string QueryText { get; }
        public VariableDictionary Variables { get; }
    }
}
