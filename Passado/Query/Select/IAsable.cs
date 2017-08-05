using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Select
{
    public interface IAsable<TJoinedRow>
    {
        IAsQuery<TNamedRow> As<TNamedRow>(Expression<Func<TJoinedRow, TNamedRow>> name);
    }
}
