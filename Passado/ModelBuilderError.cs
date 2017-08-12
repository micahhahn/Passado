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
        
        // Table errors
        public static ModelBuilderError TableRepeatedSelector(string database, string property, string otherTable) => new ModelBuilderError("PS1005", "Repeated Table Selector", $"Property '{property}' of type '{database}' has already been used as a table specification for table '{otherTable}'.");
        public static ModelBuilderError TableRepeatedName(string name) => new ModelBuilderError("PS1006", "Repeated Table Name", $"Table name '{name}' has already been used.");

        // Column errors
        public static ModelBuilderError ColumnRepeatedSelector(string table, string property, string otherColumn) => new ModelBuilderError("PS1009", "Repeated Column Selector", $"Property '{property}' of type '{table}' has already been used as a column specification for column '{otherColumn}'.");
        public static ModelBuilderError ColumnRepeatedName(string table, string name) => new ModelBuilderError("PS1010", "Repeated Column Name", $"Column name '{name}' already exists in table '{table}'.");
        public static ModelBuilderError ColumnIdentityNullable() => new ModelBuilderError("PS1011", "Nullable Identity Column", "An identity column cannot be nullable.");
        public static ModelBuilderError ColumnEnumNotStringOrIntegralType() => new ModelBuilderError("PS1012", "Enum Column Not String Or Int", "An enum must be mapped to either SqlType.Int or SqlType.String.");
        public static ModelBuilderError ColumnEnumLongerThanMaxStringSize(string maxEnumValue, int maxLength) => new ModelBuilderError("PS1013", "Enum Longer Than Max String Size", $"The enum value '{maxEnumValue}' would exceed the max length of {maxLength}.");

        // PrimaryKey errors

        // Index errors
        public static ModelBuilderError IndexClusteredAlreadySpecified(string clusteredName) => new ModelBuilderError("PSxxxx", "Clustered Index Already Specified", $"A clustered index on this table already exists: '{clusteredName}'.");
        public static ModelBuilderError IndexIncludedColumnAlreadyInKeyColumns(string columnName) => new ModelBuilderError("PSxxxx", "Index Included Column Already In Key Columns", $"The column '{columnName}' is already in the key column list so it cannot be an included column.");

        public static ModelBuilderError ArgumentNull(string argumentName) => new ModelBuilderError("PSxxxx", "Argument Null", $"The argument '{argumentName}' cannot be null.");
        
        public static ModelBuilderError SelectorInvalid(string parameterName) => new ModelBuilderError("PSxxxx", "Invalid Selector", $"A selector must be a simple property access (e.g. {parameterName}.Prop1).");
        public static ModelBuilderError OrderedSelectorInvalid(string parameterName) => new ModelBuilderError("PSxxxx", "Invalid Ordered Selector", $"An ordered selector must be an ordered property access (e.g. {parameterName}.Asc.Prop1 or {parameterName}.Desc.Prop2).");

        public static ModelBuilderError MultiSelectorInvalid(string parameterName) => new ModelBuilderError("PSxxxx", "Invalid Multi Selector", $"A multi selector must be either a simple property access (e.g. {parameterName}.Prop1) or an anonymous object of simple property accesses (e.g. new {{ {parameterName}.Prop1, {parameterName}.Prop2 }}).");
        public static ModelBuilderError OrderedMultiSelectorInvalid(string parameterName) => new ModelBuilderError("PSxxxx", "Invalid Ordered Multi Selector", $"An ordered multi selector must be either an ordered property access (e.g. {parameterName}.Prop1) or an anonymous object of ordered property accesses (e.g. new {{ {parameterName}.Prop1, {parameterName}.Prop2 }}).");

        public static ModelBuilderError SelectorNotMappedToColumn(string propertyName, string tableName) => new ModelBuilderError("PSxxxx", "Selector Not Mapped To Column", $"The property '{propertyName}' is not mapped as a column of '{tableName}'.");
        public static ModelBuilderError SelectorNotMappedToTable(string propertyName, string databaseName) => new ModelBuilderError("PSxxxx", "Selector Not Mapped To Table", $"The property '{propertyName}' is not mapped as a table of '{databaseName}'.");
    }
}