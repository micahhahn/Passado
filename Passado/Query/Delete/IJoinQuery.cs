using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Delete
{
    public interface IJoinQuery<TDatabase, TTable1, TTable2>
        : IJoinable<TDatabase, TTable1, TTable2>
        , IWhereable<IJoinedRow<TTable1, TTable2>>
        , ITerminalQuery
    {

    }

    public interface IJoinQuery<TDatabase, TTable1, TTable2, TTable3>
        : IWhereable<IJoinedRow<TTable1, TTable2, TTable3>>
        , ITerminalQuery
    {

    }
}
