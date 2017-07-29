using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Passado.Query.Select
{
    public interface IFromQuery<TDatabase, TTable1>
        : ISelectable<IJoinedRow<TTable1>>
        , IWhereable<IJoinedRow<TTable1>>
        , IGroupable<IJoinedRow<TTable1>>
    {
        IJoinQuery<TDatabase, TTable1, TTable2> Join<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector);
        IJoinQuery<TDatabase, TTable1, TTable2> Join<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition);

        IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector);
        IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition);

        IJoinQuery<TDatabase, TTable1, TTable2> RightJoin<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector);
        IJoinQuery<TDatabase, TTable1, TTable2> RightJoin<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition);

        IJoinQuery<TDatabase, TTable1, TTable2> OuterJoin<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector);
        IJoinQuery<TDatabase, TTable1, TTable2> OuterJoin<TTable2>(Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition);
    }
}
