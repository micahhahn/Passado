using System;

namespace Passado.Query
{
    public interface IGroupedRow<TJoinedRow, TKeys> : IAggregatable<TJoinedRow>
    {
        TKeys Keys { get; }
    }
}
