using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Model
{
    public enum SqlType
    {
        Binary,

        Bit,
        Boolean,
        
        Guid,
        
        // String Types
        String,

        // Date Types
        Date,
        Time,
        DateTime,
        DateTimeOffset,

        // Integral Types
        Byte,
        Short,
        Int,
        Long,

        // Floating Point Types
        Single,
        Double,

        // Fixed Point Types
        Decimal,
    }
}
