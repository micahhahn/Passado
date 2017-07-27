using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Passado.Core.Model.Table;

namespace Passado.Core.Model
{
    public class TableModel
    {
        public TableModel(string name,
                          string schema,
                          string propertyName,
                          ImmutableList<ColumnModel> columns,
                          PrimaryKeyModel primaryKey,
                          ImmutableList<ForeignKeyModel> foreignKeys,
                          ImmutableList<IndexModel> indexes)
        {
            Name = name;
            Schema = schema;
            PropertyName = propertyName;
            Columns = columns;
            PrimaryKey = primaryKey;
            ForeignKeys = foreignKeys;
            Indexes = indexes;
        }

        public string Name { get; }
        public string Schema { get; }
        public string PropertyName { get; }
        public ImmutableList<ColumnModel> Columns { get; }
        public PrimaryKeyModel PrimaryKey { get; }
        public ImmutableList<ForeignKeyModel> ForeignKeys { get; }
        public ImmutableList<IndexModel> Indexes { get; }
    }
}
