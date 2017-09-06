using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Internal
{
    public abstract class QueryBase
    {
        public QueryBase InnerQuery { get; protected set; }
    }
}
