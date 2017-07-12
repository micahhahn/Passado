using System;
using System.Collections.Generic;

using Passado.Core.Query.Select;

namespace Passado.Core.Query.Insert
{
    public interface IIntoQuery<TContext, TIntoTable>
    {
        IValuesQuery<TIntoTable> Values(TIntoTable value);
        IInsertQuery Values(IEnumerable<TIntoTable> values);
        IInsertQuery Select(Func<ISelectQueryBuilder<TContext>, ISelectQuery<TIntoTable>> query);
    }
}
