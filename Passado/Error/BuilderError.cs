using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Error
{
    public class BuilderError
    {
        protected BuilderError(string errorId, string title, string message)
        {
            ErrorId = errorId;
            Title = title;
            Message = message;
        }

        public string ErrorId { get; }
        public string Title { get; }
        public string Message { get; }

        public static BuilderError ArgumentNull(string argumentName) => new BuilderError("PSxxxx", "Argument Null", $"The argument '{argumentName}' cannot be null.");

        public static BuilderError SelectorInvalid(string parameterName) => new BuilderError("PSxxxx", "Invalid Selector", $"A selector must be a simple property access (e.g. {parameterName}.Prop1).");
        public static BuilderError OrderedSelectorInvalid(string parameterName) => new BuilderError("PSxxxx", "Invalid Ordered Selector", $"An ordered selector must be an ordered property access (e.g. {parameterName}.Asc.Prop1 or {parameterName}.Desc.Prop2).");
                      
        public static BuilderError MultiSelectorInvalid(string parameterName) => new BuilderError("PSxxxx", "Invalid Multi Selector", $"A multi selector must be either a simple property access (e.g. {parameterName}.Prop1) or an anonymous object of simple property accesses (e.g. new {{ {parameterName}.Prop1, {parameterName}.Prop2 }}).");
        public static BuilderError OrderedMultiSelectorInvalid(string parameterName) => new BuilderError("PSxxxx", "Invalid Ordered Multi Selector", $"An ordered multi selector must be either an ordered property access (e.g. {parameterName}.Prop1) or an anonymous object of ordered property accesses (e.g. new {{ {parameterName}.Prop1, {parameterName}.Prop2 }}).");
                     
        public static BuilderError SelectorNotMappedToColumn(string propertyName, string tableName) => new BuilderError("PSxxxx", "Selector Not Mapped To Column", $"The property '{propertyName}' is not mapped as a column of '{tableName}'.");
        public static BuilderError SelectorNotMappedToTable(string propertyName, string databaseName) => new BuilderError("PSxxxx", "Selector Not Mapped To Table", $"The property '{propertyName}' is not mapped as a table of '{databaseName}'.");
    }
}
