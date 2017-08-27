﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Delete
{
    public interface IAsQuery<TNamedRow>
        : IWhereable<TNamedRow>
        , ITerminalQuery
    {

    }
}
