using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Error
{
    public class ModelBuilderException : BuilderException
    {
        public ModelBuilderException(ModelBuilderError error)
            : base(error)
        {

        }
    }
}
