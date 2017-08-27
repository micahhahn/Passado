using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace Passado.Query
{
    public interface ITerminalQuery
    {
        //IQuery Build();
        //int Execute();
        //Task<int> ExecuteAsync();
    }

    public interface ITerminalQuery<TTable>
    {
        //IQuery<TTable> Build();
        //IEnumerable<TTable> Execute();
        //Task<IEnumerable<TTable>> ExecuteAsync();
    }
}
