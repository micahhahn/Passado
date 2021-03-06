﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Select
{
    public interface ISelectQuery<TResult>
        : IOrderable<TResult>
        , IOffsetable<TResult>
        , ILimitable<TResult>
        , ITerminalQuery<TResult>
    {

    }
}
