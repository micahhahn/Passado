using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
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
            var builder = new InternalTableBuilder<TDatabase, TTable>((@this as TableBuilder<TDatabase>).DatabaseBuilder)
            {
                Property = ExpressionHelpers.ParsePropertySelector(table ?? throw ModelBuilderError.ArgumentNull(nameof(table)).AsException()),
                Schema = schema
            };

            builder.Name = name ?? builder.Property.Name;
            
            {
                var priorTable = builder.DatabaseBuilder.Tables.FirstOrDefault(t => t.Property.Name == builder.Property.Name);
                if (priorTable != null)
                    throw ModelBuilderError.TableRepeatedSelector(database: typeof(TDatabase).Name, property: builder.Property.Name, otherTable: BuilderHelper.GetTableName(priorTable.Schema, priorTable.Name)).AsException();
            }
            
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
            
            var property = ExpressionHelpers.ParsePropertySelector(column ?? throw ModelBuilderError.ArgumentNull(nameof(column)).AsException());
            
            {
                var priorColumn = builder.Columns.FirstOrDefault(c => c.Property.Name == property.Name);
                if (priorColumn != null)
                    throw ModelBuilderError.ColumnRepeatedSelector(typeof(TTable).Name, property.Name, priorColumn.Name).AsException();
            }

            var columnName = name ?? property.Name;

            if (builder.Columns.Any(t => t.Name == columnName))
                throw ModelBuilderError.ColumnRepeatedName(builder.Name, columnName).AsException();

            if (nullable && identity)
                throw ModelBuilderError.ColumnIdentityNullable().AsException();
            
            if (property.Type.GetTypeInfo().IsEnum)
            {
                if (!(type == SqlType.String || SqlTypeHelpers.IsIntegral(type)))
                    throw ModelBuilderError.ColumnEnumNotStringOrIntegralType().AsException();

                var values = Enum.GetValues(property.Type);
                
                if (type == SqlType.String)
                {
                    var maxValue = values.Cast<object>()
                                         .Select(e => e.ToString())
                                         .OrderByDescending(e => e.Length)
                                         .First();

                    if (maxLength != null && maxValue.Length > maxLength)
                        throw ModelBuilderError.ColumnEnumLongerThanMaxStringSize($"{property.Type.ToString()}.{maxValue}", (int)maxLength).AsException();
                }
                else
                {

                }
            }

            builder.Columns.Add(new ColumnModel(name: columnName,
                                                property: property,
                                                sqlType: type,
                                                isNullable: nullable,
                                                isIdentity: identity,
                                                defaultValue: defaultValue));

            return builder;
        }

        public static IPrimaryKey<TDatabase, TTable> PrimaryKey<TDatabase, TTable>(this IPrimaryKeyBuilder<TDatabase, TTable> @this,
                                                                                   Expression<Func<IOrderFilter<TTable>, object>> keyColumns,
                                                                                   string name = null,
                                                                                   bool clustered = true)
        {
            var builder = @this as InternalTableBuilder<TDatabase, TTable>;
            
            var columns = ExpressionHelpers.ParseOrderedMultiPropertySelector(keyColumns ?? throw ModelBuilderError.ArgumentNull(nameof(keyColumns)).AsException())
                                           .MatchColumns(builder.Name, builder.Columns);

            var primaryKeyName = name ?? BuilderHelper.GenerateKeyName("PK", builder.Schema, builder.Name, columns.Select(c => c.Name));

            builder.PrimaryKey = new PrimaryKeyModel(primaryKeyName,
                                                     columns.ToImmutableArray(),
                                                     clustered);

            return builder;
        }

        public static IIndex<TDatabase, TTable> Index<TDatabase, TTable>(this IIndexBuilder<TDatabase, TTable> @this,
                                                                         Expression<Func<IOrderFilter<TTable>, object>> keyColumns,
                                                                         bool unique = false,
                                                                         string name = null,
                                                                         bool clustered = false,
                                                                         Expression<Func<TTable, object>> includedColumns = null)
        {
            var builder = @this as InternalTableBuilder<TDatabase, TTable>;

            var indexKeyColumns = ExpressionHelpers.ParseOrderedMultiPropertySelector(keyColumns ?? throw ModelBuilderError.ArgumentNull(nameof(keyColumns)).AsException()).MatchColumns(builder.Name, builder.Columns);
            var indexIncludedColumns = includedColumns == null ? new ImmutableArray<ColumnModel>() : ExpressionHelpers.ParseMultiPropertySelector(includedColumns).MatchColumns(builder.Name, builder.Columns);

            var indexName = name ?? BuilderHelper.GenerateKeyName("IX", builder.Schema, builder.Name, indexKeyColumns.Select(c => c.Name));

            {
                var priorClustered = builder.PrimaryKey?.IsClustered == true ? builder.PrimaryKey.Name
                                                                             : builder.Indexes.FirstOrDefault(i => i.IsClustered)?.Name;
                if (priorClustered != null)
                {
                    throw ModelBuilderError.IndexClusteredAlreadySpecified(priorClustered).AsException();
                }
            }

            {
                var column = indexIncludedColumns.FirstOrDefault(i => indexKeyColumns.Any(k => k.Property.Name == i.Property.Name));
                if (column != null)
                    throw ModelBuilderError.IndexIncludedColumnAlreadyInKeyColumns(column.Name).AsException();
            }

            builder.Indexes.Add(new IndexModel(name: indexName,
                                               keyColumns: indexKeyColumns.ToImmutableArray(),
                                               isUnique: unique,
                                               isClustered: clustered,
                                               includedColumns: indexIncludedColumns.ToImmutableArray()));

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

            var tempKeyColumns = ExpressionHelpers.ParseMultiPropertySelector(keyColumns).MatchColumns(builder.Name, builder.Columns);
            var tempReferenceTable = ExpressionHelpers.ParseSelector(referenceColumns);
            var tempReferenceColumns = ExpressionHelpers.ParseMultiPropertySelector(referenceColumns);

            var foreignKeyName = name ?? BuilderHelper.GenerateKeyName("FK", builder.Schema, builder.Name, tempKeyColumns.Select(c => c.Name));

            builder.ForeignKeys.Add(new ForeignKeyModel(name: foreignKeyName,
                                                        keyColumns: tempKeyColumns.ToImmutableArray(),
                                                        referenceTable: tempReferenceTable,
                                                        referenceColumns: tempReferenceColumns.ToImmutableArray(),
                                                        updateAction: updateAction,
                                                        deleteAction: deleteAction));

            return builder;
        }

        public static TableModel Build<TDatabase, TTable>(this ITableModelBuilder<TDatabase, TTable> @this)
        {
            var builder = @this as InternalTableBuilder<TDatabase, TTable>;

            return new TableModel(name: builder.Name,
                                  schema: builder.Schema,
                                  property: builder.Property,
                                  columns: builder.Columns.ToImmutableArray(),
                                  primaryKey: builder.PrimaryKey,
                                  foreignKeys: builder.ForeignKeys.ToImmutableArray(),
                                  indexes: builder.Indexes.ToImmutableArray());
        }
    }
}
