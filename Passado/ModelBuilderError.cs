using System;
using System.Collections.Generic;
using System.Text;

namespace Passado
{
    /// <summary>
    /// Represents an error that arises from invalid input to the model builder.  This is transformed either into a <see cref="ModelBuilderException"/> at runtime or a <see cref="Microsoft.CodeAnalysis.DiagnosticDescriptor"/> at compile time.
    /// </summary>
    public class ModelBuilderError
    {
        private ModelBuilderError(string errorId, string title, string messageFormat)
        {
            ErrorId = errorId;
            Title = title;
            MessageFormat = messageFormat;
        }

        public string ErrorId { get; }
        public string Title { get; }
        public string MessageFormat { get; }

        public static ModelBuilderError InvalidDatabaseName = new ModelBuilderError("PS1001", "Invalid Database Name", "{0}");
    }
}
