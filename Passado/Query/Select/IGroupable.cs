using System;
using System.Linq.Expressions;

namespace Passado.Query.Select
{
    public interface IGroupable<TJoinedRow>
    {
        IGroupByQuery<TJoinedRow, TKey1> GroupBy<TKey1>(Expression<Func<TJoinedRow, TKey1>> key1Selector)
            where TKey1 : IEquatable<TKey1>;

        IGroupByQuery<TJoinedRow, TKey1, TKey2> GroupBy<TKey1, TKey2>(Expression<Func<TJoinedRow, TKey1>> key1Selector,
                                                                      Expression<Func<TJoinedRow, TKey2>> key2Selector)
            where TKey1 : IEquatable<TKey1>
            where TKey2 : IEquatable<TKey2>;

        IGroupByQuery<TJoinedRow, TKey1, TKey2, TKey3> GroupBy<TKey1, TKey2, TKey3>(Expression<Func<TJoinedRow, TKey1>> key1Selector,
                                                                                    Expression<Func<TJoinedRow, TKey2>> key2Selector,
                                                                                    Expression<Func<TJoinedRow, TKey3>> key3Selector)
            where TKey1 : IEquatable<TKey1>
            where TKey2 : IEquatable<TKey2>
            where TKey3 : IEquatable<TKey3>;

        IGroupByQuery<TJoinedRow, TKey1, TKey2, TKey3, TKey4> GroupBy<TKey1, TKey2, TKey3, TKey4>(Expression<Func<TJoinedRow, TKey1>> key1Selector,
                                                                                                  Expression<Func<TJoinedRow, TKey2>> key2Selector,
                                                                                                  Expression<Func<TJoinedRow, TKey3>> key3Selector,
                                                                                                  Expression<Func<TJoinedRow, TKey4>> key4Selector)
            where TKey1 : IEquatable<TKey1>
            where TKey2 : IEquatable<TKey2>
            where TKey3 : IEquatable<TKey3>
            where TKey4 : IEquatable<TKey4>;
    }
}
