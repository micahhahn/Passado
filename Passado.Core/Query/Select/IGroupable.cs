using System;

namespace Passado.Core.Query.Select
{
    public interface IGroupable<TJoinedRow>
    {
        IGroupByQuery<TJoinedRow, TKey1> GroupBy<TKey1>(Func<TJoinedRow, TKey1> key1Selector)
            where TKey1 : IEquatable<TKey1>;

        IGroupByQuery<TJoinedRow, TKey1, TKey2> GroupBy<TKey1, TKey2>(Func<TJoinedRow, TKey1> key1Selector,
                                                                        Func<TJoinedRow, TKey2> key2Selector)
            where TKey1 : IEquatable<TKey1>
            where TKey2 : IEquatable<TKey2>;

        IGroupByQuery<TJoinedRow, TKey1, TKey2, TKey3> GroupBy<TKey1, TKey2, TKey3>(Func<TJoinedRow, TKey1> key1Selector,
                                                                                      Func<TJoinedRow, TKey2> key2Selector,
                                                                                      Func<TJoinedRow, TKey3> key3Selector)
            where TKey1 : IEquatable<TKey1>
            where TKey2 : IEquatable<TKey2>
            where TKey3 : IEquatable<TKey3>;

        IGroupByQuery<TJoinedRow, TKey1, TKey2, TKey3, TKey4> GroupBy<TKey1, TKey2, TKey3, TKey4>(Func<TJoinedRow, TKey1> key1Selector,
                                                                                                    Func<TJoinedRow, TKey2> key2Selector,
                                                                                                    Func<TJoinedRow, TKey3> key3Selector,
                                                                                                    Func<TJoinedRow, TKey4> key4Selector)
            where TKey1 : IEquatable<TKey1>
            where TKey2 : IEquatable<TKey2>
            where TKey3 : IEquatable<TKey3>
            where TKey4 : IEquatable<TKey4>;
    }
}
