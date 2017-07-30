using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Passado.Model.Table;

namespace Passado.Model
{
    public static class TableModelBuilderExtensions
    {
        public static ITable<TDatabase, TTable> Table<TDatabase, TTable>(this ITableBuilder<TDatabase> @this,
                                                                         Expression<Func<TDatabase, IEnumerable<TTable>>> table,
                                                                         string name = null,
                                                                         string schema = null)
        {
            var builder = @this as TableBuilder<TDatabase, TTable>;

            if (string.IsNullOrWhiteSpace(name))
                //throw new ModelBuilderException("");

            builder.Name = name;
            builder.Schema = schema;

            return builder;
        }

        public static IColumn<TDatabase, TTable> Column<TDatabase, TTable, TColumn>(this IColumnBuilder<TDatabase, TTable> @this,
                                                                                    Expression<Func<TTable, TColumn>> column,
                                                                                    SqlType type,
                                                                                    bool nullable = false,
                                                                                    int? maxLength = null,
                                                                                    string name = null,
                                                                                    TColumn defaultValue = default(TColumn),
                                                                                    bool identity = false,
                                                                                    IDatabaseTypeConverter<TColumn> converter = null)
        {
            var builder = @this as TableBuilder<TDatabase, TTable>;

            return builder;
        }

        public static IPrimaryKey<TDatabase, TTable> PrimaryKey<TDatabase, TTable>(this IPrimaryKeyBuilder<TDatabase, TTable> @this,
                                                                                   Expression<Func<TTable, object>> keyColumns,
                                                                                   string name = null,
                                                                                   bool clustered = true)
        {
            var builder = @this as TableBuilder<TDatabase, TTable>;

            return builder;
        }

        public static IIndex<TDatabase, TTable> Index<TDatabase, TTable>(this IIndexBuilder<TDatabase, TTable> @this,
                                                                         Expression<Func<TTable, object>> keyColumns,
                                                                         bool unique = false,
                                                                         string name = null,
                                                                         bool clustered = false,
                                                                         Expression<Func<TTable, object>> includedColumns = null)
        {
            var builder = @this as TableBuilder<TDatabase, TTable>;

            return builder;
        }

        public static IForeignKey<TDatabase, TTable> ForeignKey<TDatabase, TTable, TReference>(this IForeignKeyBuilder<TDatabase, TTable> @this,
                                                                                               Expression<Func<TTable, object>> keyColumns,
                                                                                               Expression<Func<TDatabase, IEnumerable<TReference>>> referenceTable,
                                                                                               Expression<Func<TReference, object>> referenceColumns,
                                                                                               ForeignKeyAction updateAction = ForeignKeyAction.Cascade,
                                                                                               ForeignKeyAction deleteAction = ForeignKeyAction.Cascade,
                                                                                               string name = null)
        {
            var builder = @this as TableBuilder<TDatabase, TTable>;

            return builder;
        }

        public static TableModel Build<TDatabase, TTable>(this ITableModelBuilder<TDatabase, TTable> @this)
        {
            var builder = @this as TableBuilder<TDatabase, TTable>;

            return new TableModel(name: builder.Name,
                                  schema: builder.Schema,
                                  propertyName: builder.PropertyName,
                                  columns: builder.Columns.ToImmutableList(),
                                  primaryKey: builder.PrimaryKey,
                                  foreignKeys: builder.ForeignKeys.ToImmutableList(),
                                  indexes: builder.Indexes.ToImmutableList());
        }
    }
}
