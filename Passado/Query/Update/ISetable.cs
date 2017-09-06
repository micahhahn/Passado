using System;
using System.Linq.Expressions;

namespace Passado.Query.Update
{
    public interface ISetable<TRow, TTable1>
    {
        //ISetQuery<TUpdateTable, TJoinedTable> Set<TProperty>(Expression<Func<TUpdateTable, TProperty>> property, TProperty value);
        //ISetQuery<TJoinedTable, TTable1> Set<TProperty>(Expression<Func<TTable1, TProperty>> property, Expression<Func<TJoinedTable, TProperty>> value);
    }
}
