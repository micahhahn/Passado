using System;
using System.Linq.Expressions;

namespace Passado.Core.Query.Delete
{
    public interface IWhereable<TTable>
    {
        IWhereQuery<TTable> Where(Expression<Func<TTable, bool>> condition);
    }
}
