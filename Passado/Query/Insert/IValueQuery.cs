
namespace Passado.Query.Insert
{
    public interface IValueQuery<TIntoTable>
        : IValuable<TIntoTable>
        , ITerminalQuery
    { }
}
