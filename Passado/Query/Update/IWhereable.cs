using System;
using System.Linq.Expressions;

namespace Passado.Query.Update
{
    public interface IWhereable<TJoinedTable, TTable1>
    {
        //IWhereQuery<TTable1, TJoinedTable> Where(Expression<Func<TJoinedTable, bool>> condition);
    }
}
