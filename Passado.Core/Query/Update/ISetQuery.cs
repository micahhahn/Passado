
namespace Passado.Core.Query.Update
{
    public interface ISetQuery<TUpdateTable, TJoinedTable> : ITerminalQuery, ISetable<TUpdateTable, TJoinedTable>
    {

    }
}
