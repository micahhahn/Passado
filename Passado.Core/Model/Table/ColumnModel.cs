using System.Diagnostics;

namespace Passado.Core.Model.Table
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ColumnModel
    {
        public ColumnModel(string name, string propertyName, SqlType sqlType, bool isNullable, bool isIdentity, object defaultValue)
        {
            Name = name;
            PropertyName = propertyName;
            SqlType = sqlType;
            IsNullable = isNullable;
            IsIdentity = isIdentity;
            DefaultValue = defaultValue;
        }

        public string Name { get; }
        public string PropertyName { get; }
        public SqlType SqlType { get; }
        public bool IsNullable { get; }
        public bool IsIdentity { get; }
        public object DefaultValue { get; }

        private string DebuggerDisplay => $"{Name} {SqlType}{(IsNullable ? "?" : "")}";
    }
}
