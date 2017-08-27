using System;
using System.Linq.Expressions;

namespace Passado.Query.Select
{
    public interface ISelectable<TJoinedTable>
    {
        //ISelectQuery<TResult> Select<TResult>(Expression<Func<TJoinedTable, TResult>> selector);
    }
}
