using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Select
{
    public interface IHavingable<TGroupedRow>
    {
        //IHavingQuery<TGroupedRow> Having(Expression<Func<TGroupedRow, bool>> condition);
    }
}
