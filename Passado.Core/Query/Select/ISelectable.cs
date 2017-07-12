using System;

namespace Passado.Core.Query.Select
{
    public interface ISelectable<TJoinedTable>
    {
        ISelectQuery<TResult> Select<TResult>(Func<TJoinedTable, TResult> selector);
    }
}
