using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core.Model
{
    public enum ForeignKeyAction
    {
        NoAction,
        Cascade,
        SetNull,
        SetDefault
    }
}
