
namespace Passado.Query.Insert
{
    public interface IValuesQuery<TIntoTable> : ITerminalQuery
    {
        IValuesQuery<TIntoTable> Values(TIntoTable value);
    }
}
