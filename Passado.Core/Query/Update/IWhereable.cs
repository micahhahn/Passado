using System;

namespace Passado.Core.Query.Update
{
    public interface IWhereable<TTable1, TJoinedTable>
    {
        IWhereQuery<TTable1, TJoinedTable> Where(Func<TJoinedTable, bool> condition);
    }
}
