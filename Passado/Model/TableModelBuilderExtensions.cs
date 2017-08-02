using System;
using System.Linq;
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

            var builder = new InternalTableBuilder<TDatabase, TTable>((@this as TableBuilder<TDatabase>).DatabaseBuilder);

            if (table == null)
                throw ModelBuilderError.TableNullSelector().AsException();

            builder.PropertyName = BuilderHelper.ParseSelector(table);

            if (builder.PropertyName == null)
                throw ModelBuilderError.TableInvalidSelector(database: typeof(TDatabase).Name).AsException();
            
            {
                var priorTable = builder.DatabaseBuilder.Tables.FirstOrDefault(t => t.PropertyName == builder.PropertyName);
                if (priorTable != null)
                    throw ModelBuilderError.TableRepeatedSelector(database: typeof(TDatabase).Name, property: builder.PropertyName, otherTable: BuilderHelper.GetTableName(priorTable.Schema, priorTable.Name)).AsException();
            }

            builder.Name = name != null ? name : builder.PropertyName;
            builder.Schema = schema;
            
            if (builder.DatabaseBuilder.Tables.Any(t => t.Schema == builder.Schema && t.Name == builder.Name))
                throw ModelBuilderError.TableRepeatedName(BuilderHelper.GetTableName(builder.Schema, builder.Name)).AsException();
            
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
            var builder = @this as InternalTableBuilder<TDatabase, TTable>;

            if (column == null)
                throw ModelBuilderError.ColumnNullSelector().AsException();
            
            var columnProperty = BuilderHelper.ParseSelector(column);

            if (columnProperty == null)
                throw ModelBuilderError.ColumnInvalidSelector(typeof(TTable).Name).AsException();

            {
                var priorColumn = builder.Columns.FirstOrDefault(c => c.PropertyName == columnProperty);
                if (priorColumn != null)
                    throw ModelBuilderError.ColumnRepeatedSelector(typeof(TTable).Name, columnProperty, priorColumn.Name).AsException();
            }

            var columnName = name != null ? name : columnProperty;

            if (builder.Columns.Any(t => t.Name == columnName))
                throw ModelBuilderError.ColumnRepeatedName(builder.Name, columnName).AsException();

            builder.Columns.Add(new ColumnModel(name: columnName,
                                                propertyName: columnProperty,
                                                sqlType: type,
                                                isNullable: nullable,
                                                isIdentity: identity,
                                                defaultValue: defaultValue));

            return builder;
        }

        public static IPrimaryKey<TDatabase, TTable> PrimaryKey<TDatabase, TTable>(this IPrimaryKeyBuilder<TDatabase, TTable> @this,
                                                                                   Expression<Func<TTable, object>> keyColumns,
                                                                                   string name = null,
                                                                                   bool clustered = true)
        {
            var builder = @this as InternalTableBuilder<TDatabase, TTable>;

            return builder;
        }

        public static IIndex<TDatabase, TTable> Index<TDatabase, TTable>(this IIndexBuilder<TDatabase, TTable> @this,
                                                                         Expression<Func<TTable, object>> keyColumns,
                                                                         bool unique = false,
                                                                         string name = null,
                                                                         bool clustered = false,
                                                                         Expression<Func<TTable, object>> includedColumns = null)
        {
            var builder = @this as InternalTableBuilder<TDatabase, TTable>;

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
            var builder = @this as InternalTableBuilder<TDatabase, TTable>;

            return builder;
        }

        public static TableModel Build<TDatabase, TTable>(this ITableModelBuilder<TDatabase, TTable> @this)
        {
            var builder = @this as InternalTableBuilder<TDatabase, TTable>;

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
