using System;
using System.Linq.Expressions;

namespace Passado.Query.Select
{
    public interface IGroupable<TJoinedRow>
    {
        IGroupByQuery<IGroupedRow<TJoinedRow, TKeys>> GroupBy<TKeys>(Expression<Func<TJoinedRow, TKeys>> keys);
    }
}
