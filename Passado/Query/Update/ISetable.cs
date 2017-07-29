using System;
using System.Linq.Expressions;

namespace Passado.Query.Update
{
    public interface ISetable<TUpdateTable, TJoinedTable>
    {
        ISetQuery<TUpdateTable, TJoinedTable> Set<TProperty>(Expression<Func<TUpdateTable, TProperty>> property, TProperty value);
        ISetQuery<TUpdateTable, TJoinedTable> Set<TProperty>(Expression<Func<TUpdateTable, TProperty>> property, Expression<Func<TJoinedTable, TProperty>> value);
    }
}
