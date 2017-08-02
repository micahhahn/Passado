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
        private ModelBuilderError(string errorId, string title, string message)
        {
            ErrorId = errorId;
            Title = title;
            Message = message;
        }

        public string ErrorId { get; }
        public string Title { get; }
        public string Message { get; }

        public static ModelBuilderError NullDatabaseName() => new ModelBuilderError("PS1001", "Null Database Name", "A database name cannot be null.");
        public static ModelBuilderError NullTableBuilder() => new ModelBuilderError("PS1002", "Null Table Builder", "A table builder cannot be null.");

        // Table errors
        public static ModelBuilderError TableNullSelector() => new ModelBuilderError("PS1003", "Null Table Selector", "A table selector cannot be null.");
        public static ModelBuilderError TableInvalidSelector(string database) => new ModelBuilderError("PS1004", "Invalid Table Selector", $"The table selector must be a property of '{database}'.");
        public static ModelBuilderError TableRepeatedSelector(string database, string property, string otherTable) => new ModelBuilderError("PS1005", "Repeated Table Selector", $"Property '{property}' of type '{database}' has already been used as a table specification for table '{otherTable}'.");
        public static ModelBuilderError TableRepeatedName(string name) => new ModelBuilderError("PS1006", "Repeated Table Name", $"Table name '{name}' has already been used.");

        // Column errors
        public static ModelBuilderError ColumnNullSelector() => new ModelBuilderError("PS1007", "Null Column Selector", "A column selector cannot be null.");
        public static ModelBuilderError ColumnInvalidSelector(string table) => new ModelBuilderError("PS1008", "Invalid Column Selector", $"The column selector must be a property of '{table}'.");
        public static ModelBuilderError ColumnRepeatedSelector(string table, string property, string otherColumn) => new ModelBuilderError("PS1009", "Repeated Column Selector", $"Property '{property}' of type '{table}' has already been used as a column specification for column '{otherColumn}'.");
        public static ModelBuilderError ColumnRepeatedName(string table, string name) => new ModelBuilderError("PS1010", "Repeated Column Name", $"Column name '{name}' already exists in table '{table}'.");
    }
}