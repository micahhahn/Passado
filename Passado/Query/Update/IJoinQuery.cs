using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Passado.Query.Update
{
    public interface IJoinQuery<TDatabase, TTable1, TTable2>
        : IJoinable<TDatabase, TTable1, TTable2>
        , IWhereable<IJoinedRow<TTable1, TTable2>, TTable1>
        , ISetable<IJoinedRow<TTable1, TTable2>, TTable1>
    {

    }

    public interface IJoinQuery<TDatabase, TTable1, TTable2, TTable3>
        : IWhereable<IJoinedRow<TTable1, TTable2, TTable3>, TTable1>
        , ISetable<IJoinedRow<TTable1, TTable2, TTable3>, TTable1>
    {

    }
}
