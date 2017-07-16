using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core.Query
{
    public interface ITerminalQuery
    {
        IQuery Build();
        int Execute();
    }

    public interface ITerminalQuery<TTable>
    {
        IQuery<TTable> Build();
        IEnumerable<TTable> Execute();
    }
}
