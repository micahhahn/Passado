using System;
using System.Collections.Generic;
using System.Text;

namespace Passado
{
    public static class ModelBuilderErrorExtensions
    {
        public static ModelBuilderException AsException(this ModelBuilderError error)
        {
            return new ModelBuilderException(error);
        }
    }
}
