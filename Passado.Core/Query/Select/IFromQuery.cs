using System;
using System.Collections.Generic;

namespace Passado.Core.Query.Select
{
    public interface IFromQuery<TDatabase, TTable1>
        : ISelectable<IJoinedRow<TTable1>>
        , IWhereable<IJoinedRow<TTable1>>
        , IGroupable<IJoinedRow<TTable1>>
    {
        IJoinQuery<TDatabase, TTable1, TTable2> Join<TTable2>(Func<TDatabase, IEnumerable<TTable2>> selector);
        IJoinQuery<TDatabase, TTable1, TTable2> Join<TTable2>(Func<TDatabase, IEnumerable<TTable2>> selector, Func<IJoinedRow<TTable1, TTable2>, bool> condition);

        IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TTable2>(Func<TDatabase, IEnumerable<TTable2>> selector);
        IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TTable2>(Func<TDatabase, IEnumerable<TTable2>> selector, Func<IJoinedRow<TTable1, TTable2>, bool> condition);

        IJoinQuery<TDatabase, TTable1, TTable2> RightJoin<TTable2>(Func<TDatabase, IEnumerable<TTable2>> selector);
        IJoinQuery<TDatabase, TTable1, TTable2> RightJoin<TTable2>(Func<TDatabase, IEnumerable<TTable2>> selector, Func<IJoinedRow<TTable1, TTable2>, bool> condition);

        IJoinQuery<TDatabase, TTable1, TTable2> OuterJoin<TTable2>(Func<TDatabase, IEnumerable<TTable2>> selector);
        IJoinQuery<TDatabase, TTable1, TTable2> OuterJoin<TTable2>(Func<TDatabase, IEnumerable<TTable2>> selector, Func<IJoinedRow<TTable1, TTable2>, bool> condition);
    }
}
