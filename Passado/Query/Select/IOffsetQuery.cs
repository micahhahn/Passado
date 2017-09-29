using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Select
{
    public interface IOffsetQuery<TResult>
        : ILimitable<TResult>
        , ITerminalQuery<TResult>
    {
    }
}
