using System;

namespace Passado.Core.Query.Delete
{
    public interface IWhereable<TTable>
    {
        IWhereQuery<TTable> Where(Func<TTable, bool> condition);
    }
}
