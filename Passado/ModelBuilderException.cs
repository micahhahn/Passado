using System;
using System.Collections.Generic;
using System.Text;

namespace Passado
{
    public class ModelBuilderException : Exception
    {
        private ModelBuilderException()
        {
            throw new NotImplementedException();
        }

        public ModelBuilderException(ModelBuilderError error, string message)
            : base(string.Format(error.MessageFormat, message))
        {
            ErrorId = error.ErrorId;
            Title = error.Title;
        }

        public string ErrorId { get; }
        public string Title { get; }
    }
}
