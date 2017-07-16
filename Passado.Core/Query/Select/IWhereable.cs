using System;
using System.Linq.Expressions;

namespace Passado.Core.Query.Select
{
    public interface IWhereable<TJoinedRow>
    {
        IWhereQuery<TJoinedRow> Where(Expression<Func<TJoinedRow, bool>> condition);
    }
}
