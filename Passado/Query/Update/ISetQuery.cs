
namespace Passado.Query.Update
{
    public interface ISetQuery<TRow, TTable1>
        : ISetable<TRow, TTable1>
        , ITerminalQuery
    {

    }
}
