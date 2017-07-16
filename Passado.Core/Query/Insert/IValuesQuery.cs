
namespace Passado.Core.Query.Insert
{
    public interface IValuesQuery<TIntoTable> : ITerminalQuery
    {
        IValuesQuery<TIntoTable> Values(TIntoTable value);
    }
}
