using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Passado.Model.Table;

namespace Passado.Model
{
    public class TableModel
    {
        public TableModel(string name,
                          string schema,
                          PropertyModel property,
                          ImmutableArray<ColumnModel> columns,
                          PrimaryKeyModel primaryKey,
                          ImmutableArray<ForeignKeyModel> foreignKeys,
                          ImmutableArray<IndexModel> indexes)
        {
            Name = name;
            Schema = schema;
            Property = property;
            Columns = columns;
            PrimaryKey = primaryKey;
            ForeignKeys = foreignKeys;
            Indexes = indexes;
        }

        public string Name { get; }
        public string Schema { get; }
        public PropertyModel Property { get; }
        public ImmutableArray<ColumnModel> Columns { get; }
        public PrimaryKeyModel PrimaryKey { get; }
        public ImmutableArray<ForeignKeyModel> ForeignKeys { get; }
        public ImmutableArray<IndexModel> Indexes { get; }
    }
}
