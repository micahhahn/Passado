using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Linq;
using System.Text;

namespace Passado.Core.Model.Builder
{
    public class ForeignKeyOrIndexBuilder<TDatabase, TTable>
    {
        public ColumnOrPrimaryKeyBuilder<TDatabase, TTable> TableAndColumns { get; set; }
        public PrimaryKeyModel PrimaryKey { get; set; }
        public List<ForeignKeyModel> ForeignKeys { get; set; }
        public List<IndexModel> Indexes { get; set; }

        public ForeignKeyOrIndexBuilder<TDatabase, TTable> ForeignKey<TReference>(Expression<Func<TTable, object>> keyColumns,
                                                                                  Expression<Func<TDatabase, IEnumerable<TReference>>> referenceTable,
                                                                                  Expression<Func<TReference, object>> referenceColumns,
                                                                                  ForeignKeyAction updateAction = ForeignKeyAction.Cascade,
                                                                                  ForeignKeyAction deleteAction = ForeignKeyAction.Cascade,
                                                                                  string name = null)
        {
            var keyColumnModels = Builder.ParseColumnSelector(keyColumns, TableAndColumns.Columns).ToImmutableList();

            if ((updateAction == ForeignKeyAction.SetNull || deleteAction == ForeignKeyAction.SetNull) &&
                keyColumnModels.Any(k => k.IsNullable == false))
                throw new ModelException($"All key columns must be nullable if 'SetNull' is a foreign key action.");

            if ((updateAction == ForeignKeyAction.SetDefault || deleteAction == ForeignKeyAction.SetDefault) &&
                keyColumnModels.Any(k => k.DefaultValue == null && k.IsIdentity == false))
                throw new ModelException($"All key columns must have a default value if 'SetDefault' is a foreign key action.");

            ForeignKeys.Add(new ForeignKeyModel()
            {
                Name = name,
                UpdateAction = updateAction,
                DeleteAction = deleteAction,
                KeyColumns = keyColumnModels,
                ReferenceTableExpression = referenceTable,
                ReferenceColumnsExpression = referenceColumns,
            });

            return this;
        }

        public ForeignKeyOrIndexBuilder<TDatabase, TTable> Index(Expression<Func<TTable, object>> keyColumns, bool unique = false, string name = null, bool clustered = false, Expression<Func<TTable, object>> includedColumns = null)
        {
            var keyColumnsTemp = Builder.ParseOrderedColumnSelector(keyColumns, TableAndColumns.Columns);

            var indexName = name ?? $"{(unique ? "UX" : "IX")}_{(TableAndColumns.Schema != null ? $"{TableAndColumns.Schema}_" : "")}{TableAndColumns.Name}_{string.Join("_", keyColumnsTemp.Select(c => c.ColumnName))}";

            Indexes.Add(new IndexModel(name: indexName,
                                  keyColumns: keyColumnsTemp,
                                  isUnique: unique,
                                  isClustered: clustered,
                                  includedColumns: includedColumns == null ? null : Builder.ParseColumnSelector(includedColumns, TableAndColumns.Columns)));

            return this;
        }

        public TableModel Build()
        {
            return new TableModel()
            {
                Name = TableAndColumns.Name,
                Schema = TableAndColumns.Schema,
                PropertyName = TableAndColumns.PropertyName,
                PropertyType = TableAndColumns.PropertyType,
                Columns = TableAndColumns.Columns.ToImmutableList(),
                PrimaryKey = PrimaryKey,
                ForeignKeys = ForeignKeys.ToImmutableList(),
                Indexes = Indexes.ToImmutableList()
            };
        }
    }
}
