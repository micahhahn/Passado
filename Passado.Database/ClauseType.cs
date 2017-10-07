using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Database
{
    public enum ClauseType
    {
        Insert,
        Values,
        Select,
        Update,
        Set,
        Delete,
        From,
        Join,
        Where,
        GroupBy,
        Having,
        OrderBy,
        Limit,
        Offset
    }
}
