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

        public ModelBuilderException(ModelBuilderError error)
            : base(error.Message)
        {
            ErrorId = error.ErrorId;
            Title = error.Title;
        }

        public string ErrorId { get; }
        public string Title { get; }
    }
}
