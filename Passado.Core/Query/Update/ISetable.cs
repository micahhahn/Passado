using System;

namespace Passado.Core.Query.Update
{
    public interface ISetable<TUpdateTable, TJoinedTable>
    {
        ISetQuery<TUpdateTable, TJoinedTable> Set<TProperty>(Func<TUpdateTable, TProperty> property, TProperty value);
        ISetQuery<TUpdateTable, TJoinedTable> Set<TProperty>(Func<TUpdateTable, TProperty> property, Func<TJoinedTable, TProperty> value);
    }
}
