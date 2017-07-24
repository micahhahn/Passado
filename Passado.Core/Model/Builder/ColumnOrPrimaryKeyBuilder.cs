using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;

namespace Passado.Core.Model.Builder
{
    public class ColumnOrPrimaryKeyBuilder<TDatabase, TTable> : ColumnModelBuilderBase<TDatabase, TTable>
    {
        public List<ColumnModel> Columns { get; set; }

        public ForeignKeyOrIndexBuilder<TDatabase, TTable> PrimaryKey(Expression<Func<TTable, object>> keyColumns, string name = null, bool clustered = true)
        {
            var columnModels = Builder.ParseOrderedColumnSelector(keyColumns, Columns);

            var primaryKeyName = name ?? $"PK_{(Schema != null ? $"{Schema}_" : "")}{Name}_{string.Join("_", columnModels.Select(c => c.ColumnName))}";

            return new ForeignKeyOrIndexBuilder<TDatabase, TTable>()
            {
                TableAndColumns = this,
                PrimaryKey = new PrimaryKeyModel(name: primaryKeyName, columns: columnModels, isClustered: clustered),
                ForeignKeys = new List<ForeignKeyModel>(),
                Indexes = new List<IndexModel>()
            };
        }
    }
}
