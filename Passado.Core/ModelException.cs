using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core
{
    public class ModelException : Exception
    {
        public ModelException(string message)
            : base(message)
        {

        }
    }
}
