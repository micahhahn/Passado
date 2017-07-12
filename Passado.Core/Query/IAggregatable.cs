using System;

namespace Passado.Core.Query
{
    public interface IAggregatable<TJoinedRow>
    {
        int Count();
        int Count<TResult>(Func<TJoinedRow, TResult> selector);
        int CountDistinct<TResult>(Func<TJoinedRow, TResult> selector);

        TResult Min<TResult>(Func<TJoinedRow, TResult> selector);
        TResult Max<TResult>(Func<TJoinedRow, TResult> selector);
    }
}
