using System;
using System.Linq.Expressions;

namespace Passado.Query.Update
{
    public interface IWhereable<TTable1, TJoinedTable>
    {
        IWhereQuery<TTable1, TJoinedTable> Where(Expression<Func<TJoinedTable, bool>> condition);
    }
}
