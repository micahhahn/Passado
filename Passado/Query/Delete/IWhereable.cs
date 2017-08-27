using System;
using System.Linq.Expressions;

namespace Passado.Query.Delete
{
    public interface IWhereable<TTable>
    {
        //IWhereQuery<TTable> Where(Expression<Func<TTable, bool>> condition);
    }
}
