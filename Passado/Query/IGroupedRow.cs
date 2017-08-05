using System;

namespace Passado.Query
{
    public interface IGroupedRow<TJoinedRow, TKey1> : IAggregatable<TJoinedRow>
    {
        TKey1 Key1 { get; }
    }

    public interface IGroupedRow<TJoinedRow, TKey1, TKey2> : IAggregatable<TJoinedRow>
    {
        TKey1 Key1 { get; }
        TKey2 Key2 { get; }
    }

    public interface IGroupedRow<TJoinedRow, TKey1, TKey2, TKey3> : IAggregatable<TJoinedRow>
    {
        TKey1 Key1 { get; }
        TKey2 Key2 { get; }
        TKey3 Key3 { get; }
    }

    public interface IGroupedRow<TJoinedRow, TKey1, TKey2, TKey3, TKey4> : IAggregatable<TJoinedRow>
    {
        TKey1 Key1 { get; }
        TKey2 Key2 { get; }
        TKey3 Key3 { get; }
        TKey4 Key4 { get; }
    }
}
