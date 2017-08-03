﻿using System;
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
                          ImmutableList<ColumnModel> columns,
                          PrimaryKeyModel primaryKey,
                          ImmutableList<ForeignKeyModel> foreignKeys,
                          ImmutableList<IndexModel> indexes)
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
        public ImmutableList<ColumnModel> Columns { get; }
        public PrimaryKeyModel PrimaryKey { get; }
        public ImmutableList<ForeignKeyModel> ForeignKeys { get; }
        public ImmutableList<IndexModel> Indexes { get; }
    }
}
