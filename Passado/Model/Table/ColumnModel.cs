using System.Diagnostics;

namespace Passado.Model.Table
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ColumnModel
    {
        public ColumnModel(string name, PropertyModel property, SqlType sqlType, bool isNullable, bool isIdentity, object defaultValue)
        {
            Name = name;
            Property = property;
            SqlType = sqlType;
            IsNullable = isNullable;
            IsIdentity = isIdentity;
            DefaultValue = defaultValue;
        }

        public string Name { get; }
        public PropertyModel Property { get; }
        public SqlType SqlType { get; }
        public bool IsNullable { get; }
        public bool IsIdentity { get; }
        public object DefaultValue { get; }

        private string DebuggerDisplay => $"{Name} {SqlType}{(IsNullable ? "?" : "")}";
    }
}
