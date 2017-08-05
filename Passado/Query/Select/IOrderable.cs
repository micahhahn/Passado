using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Select
{
    public interface IOrderable<TResult>
    {
        ITerminalQuery<TResult> OrderBy(Expression<Func<IOrderFilter<TResult>, object>> selector);
    }
}
