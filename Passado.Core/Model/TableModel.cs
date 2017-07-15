using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Passado.Core.Model
{
    public class TableModel
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }
        public ImmutableList<ColumnModel> Columns { get; set; }
        public PrimaryKeyModel PrimaryKey { get; set; }
        public ImmutableList<ForeignKeyModel> ForeignKeys { get; set; }
        public ImmutableList<IndexModel> Indexes { get; set; }
    }
}
