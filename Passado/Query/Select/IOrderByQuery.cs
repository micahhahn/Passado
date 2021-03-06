﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Select
{
    public interface IOrderByQuery<TResult>
        : IOffsetable<TResult>
        , ILimitable<TResult>
        , ITerminalQuery<TResult>
    {

    }
}
