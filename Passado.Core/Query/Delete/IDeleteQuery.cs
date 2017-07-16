
namespace Passado.Core.Query.Delete
{
    public interface IDeleteQuery<TContext, TTable1> : IWhereable<IJoinedRow<TTable1>>, ITerminalQuery
    {

    }
}
