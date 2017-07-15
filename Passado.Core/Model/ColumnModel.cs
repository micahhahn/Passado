using System.Diagnostics;

namespace Passado.Core.Model
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ColumnModel
    {
        private readonly string _propertyName;
        private readonly string _columnName;
        private readonly SqlType _sqlType;
        private readonly bool _isNullable;
        private readonly bool _isIdentity;
        private readonly object _defaultValue;

        public ColumnModel(string propertyName, string columnName, SqlType sqlType, bool isNullable, bool isIdentity, object defaultValue)
        {
            _propertyName = propertyName;
            _columnName = columnName;
            _sqlType = sqlType;
            _isNullable = isNullable;
            _isIdentity = isIdentity;
            _defaultValue = defaultValue;
        }

        public string PropertyName => _propertyName;
        public string ColumnName => _columnName;
        public SqlType SqlType => _sqlType;
        public bool IsNullable => _isNullable;
        public bool IsIdentity => _isIdentity;
        public object DefaultValue => _defaultValue;

        private string DebuggerDisplay => $"{ColumnName} {SqlType}{(IsNullable ? "?" : "")}";
    }
}
