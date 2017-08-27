using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Update
{
    public interface IAsQuery<TNamedRow, TUpdateTable>
        : IWhereable<TNamedRow, TUpdateTable>
        , ISetable<TNamedRow, TUpdateTable>
    {

    }
}
