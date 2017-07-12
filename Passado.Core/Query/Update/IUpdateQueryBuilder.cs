using System;
using System.Collections.Generic;

namespace Passado.Core.Query.Update
{
    public interface IUpdateQueryBuilder<TContext>
    {
        IFromQuery<TContext, TTable1> From<TTable1>(Func<TContext, IEnumerable<TTable1>> selector);
    }
}
