using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Model
{
    public static class SqlTypeHelpers
    {
        public static bool IsIntegral(SqlType type)
        {
            switch (type)
            {
                case SqlType.Byte:
                case SqlType.Short:
                case SqlType.Int:
                case SqlType.Long:
                    return true;
                default:
                    return false;
            }
        }
    }
}
