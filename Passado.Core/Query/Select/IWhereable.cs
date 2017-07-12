using System;

namespace Passado.Core.Query.Select
{
    public interface IWhereable<TJoinedRow>
    {
        IWhereQuery<TJoinedRow> Where(Func<TJoinedRow, bool> condition);
    }
}
