using System;
using System.Collections.Generic;

namespace Passado.Core.Query.Delete
{
    public interface IDeleteQueryBuilder<TDatabase>
    {
        IFromQuery<TDatabase, TRow1> From<TRow1>(Func<TDatabase, IEnumerable<TRow1>> selector);
    }
}
