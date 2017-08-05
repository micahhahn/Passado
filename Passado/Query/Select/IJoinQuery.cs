using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Passado.Query.Select
{
    public interface IJoinQuery<TDatabase, TTable1, TTable2>
        : ISelectable<IJoinedRow<TTable1, TTable2>>
        , IWhereable<IJoinedRow<TTable1, TTable2>>
        , IGroupable<IJoinedRow<TTable1, TTable2>>
        , IAsable<IJoinedRow<TTable1, TTable2>>
    {
        IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TTable3>(Expression<Func<TDatabase, IEnumerable<TTable3>>> selector);
        IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TTable3>(Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition);

        IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TTable3>(Expression<Func<TDatabase, IEnumerable<TTable3>>> selector);
        IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TTable3>(Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition);

        IJoinQuery<TDatabase, TTable1, TTable2, TTable3> RightJoin<TTable3>(Expression<Func<TDatabase, IEnumerable<TTable3>>> selector);
        IJoinQuery<TDatabase, TTable1, TTable2, TTable3> RightJoin<TTable3>(Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition);

        IJoinQuery<TDatabase, TTable1, TTable2, TTable3> OuterJoin<TTable3>(Expression<Func<TDatabase, IEnumerable<TTable3>>> selector);
        IJoinQuery<TDatabase, TTable1, TTable2, TTable3> OuterJoin<TTable3>(Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition);
    }

    public interface IJoinQuery<TContext, TTable1, TTable2, TTable3>
        : ISelectable<IJoinedRow<TTable1, TTable2, TTable3>>
        , IWhereable<IJoinedRow<TTable1, TTable2, TTable3>>
        , IGroupable<IJoinedRow<TTable1, TTable2, TTable3>>
        , IAsable<IJoinedRow<TTable1, TTable2, TTable3>>
    {

    }
}
