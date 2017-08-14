using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Error
{
    /// <summary>
    /// Represents an error that arises from invalid input to the model builder.  This is transformed either into a <see cref="ModelBuilderException"/> at runtime or a <see cref="Microsoft.CodeAnalysis.DiagnosticDescriptor"/> at compile time.
    /// </summary>
    public class ModelBuilderError : BuilderError
    {
        private ModelBuilderError(string errorId, string title, string message)
            : base(errorId, title, message)
        {

        }

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

        // Foreign Key errors
        public static ModelBuilderError ForeignKeyColumnCountsDontMatch() => new ModelBuilderError("PSxxxx", "Foreign Key Column Counts Dont Match", $"There must be the same number of key columns and reference columns.");
        public static ModelBuilderError ForeignKeyColumnTypesDontMatch(string foreignKeyName, string keyColumnName, string keyColumnType, string referenceColumnName, string referenceColumnType) => new ModelBuilderError("PSxxxx", "Foreign Key Column Types Dont Match", $"In foreign key '{foreignKeyName}' the type of key column '{keyColumnName}' ('{keyColumnType}') does not match the type of reference column '{referenceColumnName}' ('{referenceColumnType}').");
    }
}