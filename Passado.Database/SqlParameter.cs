using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

namespace Passado.Database
{
    public class SqlParameter
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public ConstantExpression Expression { get; set; }
    }
}
