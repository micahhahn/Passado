
namespace Passado.Core.Query.Delete
{
    public interface IFromQuery<TContext, TTable1> : IWhereable<IJoinedRow<TTable1>>, IDeleteQuery
    {

    }
}
