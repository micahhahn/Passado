using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Error
{
    public class QueryBuilderException : BuilderException
    {
        public QueryBuilderException(QueryBuilderError error)
            : base(error)
        {

        }
    }
}
