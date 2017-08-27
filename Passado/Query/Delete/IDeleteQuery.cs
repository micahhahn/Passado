using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Passado.Query.Delete
{
    public interface IDeleteQuery<TDatabase, TTable1> 
        : IJoinable<TDatabase, TTable1>
        , IWhereable<IJoinedRow<TTable1>>
        , ITerminalQuery
    {
        //IJoinQuery<TDatabase, TTable1, TTable2> Join<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector);
        //IJoinQuery<TDatabase, TTable1, TTable2> Join<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition);

        //IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector);
        //IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition);
    }
}
