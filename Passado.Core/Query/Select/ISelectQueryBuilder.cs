using System;
using System.Collections.Generic;

namespace Passado.Core.Query.Select
{
    public interface ISelectQueryBuilder<TContext>
    {
        IFromQuery<TContext, TTable1> From<TTable1>(Func<TContext, IEnumerable<TTable1>> selector);
    }
}
