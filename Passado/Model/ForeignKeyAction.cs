using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Model
{
    public enum ForeignKeyAction
    {
        NoAction,
        Cascade,
        SetNull,
        SetDefault
    }
}
