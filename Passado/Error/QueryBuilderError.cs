using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Error
{
    public class QueryBuilderError : BuilderError
    {
        public QueryBuilderError(string errorId, string title, string message)
            : base(errorId, title, message)
        {

        }
    }
}
