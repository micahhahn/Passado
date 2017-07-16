using System;
using System.Linq.Expressions;

namespace Passado.Core.Query.Select
{
    public interface ISelectable<TJoinedTable>
    {
        ITerminalQuery<TResult> Select<TResult>(Expression<Func<TJoinedTable, TResult>> selector);
    }
}
