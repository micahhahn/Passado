using System;
using System.Collections.Generic;

namespace Passado.Core.Query.Update
{
    public interface IJoinQuery<TContext, TTable1, TTable2> : IWhereable<TTable1, IJoinedRow<TTable1, TTable2>>, ISetable<TTable1, IJoinedRow<TTable1, TTable2>>
    {
        IJoinQuery<TContext, TTable1, TTable2, TTable3> Join<TTable3>(Func<TContext, IEnumerable<TTable3>> selector);
        IJoinQuery<TContext, TTable1, TTable2, TTable3> Join<TTable3>(Func<TContext, IEnumerable<TTable3>> selector, Func<IJoinedRow<TTable1, TTable2, TTable3>, bool> condition);

        IJoinQuery<TContext, TTable1, TTable2, TTable3> LeftJoin<TTable3>(Func<TContext, IEnumerable<TTable3>> selector);
        IJoinQuery<TContext, TTable1, TTable2, TTable3> LeftJoin<TTable3>(Func<TContext, IEnumerable<TTable3>> selector, Func<IJoinedRow<TTable1, TTable2, TTable3>, bool> condition);
    }

    public interface IJoinQuery<TContext, TTable1, TTable2, TTable3> : IWhereable<TTable1, IJoinedRow<TTable1, TTable2, TTable3>>, ISetable<TTable1, IJoinedRow<TTable1, TTable2, TTable3>>
    {

    }
}
