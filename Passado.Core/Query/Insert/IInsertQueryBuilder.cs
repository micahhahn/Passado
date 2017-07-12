using System;
using System.Collections.Generic;

namespace Passado.Core.Query.Insert
{
    public interface IInsertQueryBuilder<TDatabase>
    {
        IIntoQuery<TDatabase, TIntoTable> Into<TIntoTable>(Func<TDatabase, IEnumerable<TIntoTable>> selector, Func<TIntoTable, object> columns);
    }
}
