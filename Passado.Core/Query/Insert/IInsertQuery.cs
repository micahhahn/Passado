using System;
using System.Collections.Generic;

using Passado.Core.Query.Select;

namespace Passado.Core.Query.Insert
{
    public interface IInsertQuery<TContext, TIntoTable>
    {
        IValuesQuery<TIntoTable> Values(TIntoTable value);
        ITerminalQuery Values(IEnumerable<TIntoTable> values);
        //IInsertQuery Select(Func<ISelectQueryBuilder<TContext>, ISelectQuery<TIntoTable>> query);
    }
}
