using System;
using System.Linq.Expressions;

namespace Passado.Query.Select
{
    public interface IGroupable<TJoinedRow>
    {
        IGroupByQuery<IGroupedRow<TJoinedRow, TKey1>> GroupBy<TKey1>(Expression<Func<TJoinedRow, TKey1>> key1Selector);

        IGroupByQuery<IGroupedRow<TJoinedRow, TKey1, TKey2>> GroupBy<TKey1, TKey2>(Expression<Func<TJoinedRow, TKey1>> key1Selector,
                                                                      Expression<Func<TJoinedRow, TKey2>> key2Selector);

        IGroupByQuery<IGroupedRow<TJoinedRow, TKey1, TKey2, TKey3>> GroupBy<TKey1, TKey2, TKey3>(Expression<Func<TJoinedRow, TKey1>> key1Selector,
                                                                                    Expression<Func<TJoinedRow, TKey2>> key2Selector,
                                                                                    Expression<Func<TJoinedRow, TKey3>> key3Selector);

        IGroupByQuery<IGroupedRow<TJoinedRow, TKey1, TKey2, TKey3, TKey4>> GroupBy<TKey1, TKey2, TKey3, TKey4>(Expression<Func<TJoinedRow, TKey1>> key1Selector,
                                                                                                  Expression<Func<TJoinedRow, TKey2>> key2Selector,
                                                                                                  Expression<Func<TJoinedRow, TKey3>> key3Selector,
                                                                                                  Expression<Func<TJoinedRow, TKey4>> key4Selector);
    }
}
