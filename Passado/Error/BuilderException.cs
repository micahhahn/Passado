using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Error
{
    public class BuilderException : Exception
    {
        public BuilderException(BuilderError error)
            : base(error.Message)
        {
            ErrorId = error.ErrorId;
            Title = error.Title;
        }

        public string ErrorId { get; }
        public string Title { get; }
    }
}
