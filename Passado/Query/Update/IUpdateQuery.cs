using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Passado.Query.Update
{
    public interface IUpdateQuery<TDatabase, TTable1> 
        : IJoinable<TDatabase, TTable1>
        , IWhereable<TTable1, IJoinedRow<TTable1>>
        , ISetable<TTable1, IJoinedRow<TTable1>>
    {
        //IJoinQuery<TDatabase, TTable1, TTable2> Join<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector);
        //IJoinQuery<TDatabase, TTable1, TTable2> Join<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition);

        //IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector);
        //IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition);
    }
}
