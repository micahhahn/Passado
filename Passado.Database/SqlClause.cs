using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Database
{
    public class SqlClause
    {
        public SqlClause(ClauseType clauseType, string clauseText)
        {
            ClauseType = clauseType;
            ClauseText = clauseText;
        }

        public ClauseType ClauseType { get; }
        public string ClauseText { get; }
    }
}
