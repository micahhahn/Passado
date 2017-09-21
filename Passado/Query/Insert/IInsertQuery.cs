using System;
using System.Collections.Generic;

using Passado.Query.Select;

namespace Passado.Query.Insert
{
    public interface IInsertQuery<TContext, TIntoTable>
        : IValuable<TIntoTable>
    {
        //IInsertQuery Select(Func<ISelectQueryBuilder<TContext>, ISelectQuery<TIntoTable>> query);
    }
}
