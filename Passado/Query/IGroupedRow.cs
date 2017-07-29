using System;

namespace Passado.Query
{
    public interface IGroupedRow<TJoinedRow, TKey1> : IAggregatable<TJoinedRow>
        where TKey1 : IEquatable<TKey1>
    {
        TKey1 Key1 { get; }
    }

    public interface IGroupedRow<TJoinedRow, TKey1, TKey2> : IAggregatable<TJoinedRow>
        where TKey1 : IEquatable<TKey1>
        where TKey2 : IEquatable<TKey2>
    {
        TKey1 Key1 { get; }
        TKey2 Key2 { get; }
    }

    public interface IGroupedRow<TJoinedRow, TKey1, TKey2, TKey3> : IAggregatable<TJoinedRow>
        where TKey1 : IEquatable<TKey1>
        where TKey2 : IEquatable<TKey2>
        where TKey3 : IEquatable<TKey3>
    {
        TKey1 Key1 { get; }
        TKey2 Key2 { get; }
        TKey3 Key3 { get; }
    }

    public interface IGroupedRow<TJoinedRow, TKey1, TKey2, TKey3, TKey4> : IAggregatable<TJoinedRow>
        where TKey1 : IEquatable<TKey1>
        where TKey2 : IEquatable<TKey2>
        where TKey3 : IEquatable<TKey3>
        where TKey4 : IEquatable<TKey4>
    {
        TKey1 Key1 { get; }
        TKey2 Key2 { get; }
        TKey3 Key3 { get; }
        TKey4 Key4 { get; }
    }
}
