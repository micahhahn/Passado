using System;

namespace Passado.Query.Select
{
    public interface IGroupByQuery<TJoinedRow, TKey1> : ISelectable<IGroupedRow<TJoinedRow, TKey1>>
        where TKey1 : IEquatable<TKey1>
    {

    }

    public interface IGroupByQuery<TJoinedRow, TKey1, TKey2> : ISelectable<IGroupedRow<TJoinedRow, TKey1, TKey2>>
        where TKey1 : IEquatable<TKey1>
        where TKey2 : IEquatable<TKey2>
    {

    }

    public interface IGroupByQuery<TJoinedRow, TKey1, TKey2, TKey3> : ISelectable<IGroupedRow<TJoinedRow, TKey1, TKey2, TKey3>>
        where TKey1 : IEquatable<TKey1>
        where TKey2 : IEquatable<TKey2>
        where TKey3 : IEquatable<TKey3>
    {

    }

    public interface IGroupByQuery<TJoinedRow, TKey1, TKey2, TKey3, TKey4> : ISelectable<IGroupedRow<TJoinedRow, TKey1, TKey2, TKey3, TKey4>>
        where TKey1 : IEquatable<TKey1>
        where TKey2 : IEquatable<TKey2>
        where TKey3 : IEquatable<TKey3>
        where TKey4 : IEquatable<TKey4>
    {

    }
}
