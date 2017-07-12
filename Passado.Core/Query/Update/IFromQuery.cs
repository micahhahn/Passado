using System;
using System.Collections.Generic;

namespace Passado.Core.Query.Update
{
    public interface IFromQuery<TDatabase, TTable1> : ISetable<TTable1, IJoinedRow<TTable1>>, IWhereable<TTable1, IJoinedRow<TTable1>>
    {
        IJoinQuery<TDatabase, TTable1, TTable2> Join<TTable2>(Func<TDatabase, IEnumerable<TTable2>> selector);
        IJoinQuery<TDatabase, TTable1, TTable2> Join<TTable2>(Func<TDatabase, IEnumerable<TTable2>> selector, Func<IJoinedRow<TTable1, TTable2>, bool> condition);

        IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TTable2>(Func<TDatabase, IEnumerable<TTable2>> selector);
        IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TTable2>(Func<TDatabase, IEnumerable<TTable2>> selector, Func<IJoinedRow<TTable1, TTable2>, bool> condition);
    }
}
