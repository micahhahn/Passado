using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core.Query.Delete
{
    public interface IJoinQuery<TDatabase, TTable1, TTable2>
        : IWhereable<IJoinedRow<TTable1, TTable2>>
        , ITerminalQuery
    {

    }
}
