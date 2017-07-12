
namespace Passado.Core.Query.Insert
{
    public interface IValuesQuery<TIntoTable> : IInsertQuery
    {
        IValuesQuery<TIntoTable> Values(TIntoTable value);
    }
}
