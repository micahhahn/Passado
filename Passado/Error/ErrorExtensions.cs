using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Error
{
    public static class ErrorExtensions
    {
        public static BuilderException AsException(this BuilderError error)
        {
            return new BuilderException(error);
        }

        public static ModelBuilderException AsException(this ModelBuilderError error)
        {
            return new ModelBuilderException(error);
        }

        public static QueryBuilderException AsException(this QueryBuilderError error)
        {
            return new QueryBuilderException(error);
        }
    }
}
