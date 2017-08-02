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
                throw new ModelBuilderException(ModelBuilderError.InvalidTableSelector, "The table selector cannot be null.");

            builder.PropertyName = BuilderHelper.ParseSelector(table);

            if (builder.PropertyName == null)
                throw new ModelBuilderException(ModelBuilderError.InvalidTableSelector, $"The table selector must be a property of '{typeof(TDatabase).Name}'.");

            {
                var priorTable = builder.DatabaseBuilder.Tables.FirstOrDefault(t => t.PropertyName == builder.PropertyName);
                if (priorTable != null)
                    throw new ModelBuilderException(ModelBuilderError.InvalidTableSelector, $"Property '{builder.PropertyName}' of type '{typeof(TDatabase).Name}' has already been used as a table specification for table '{BuilderHelper.GetTableName(priorTable.Schema, priorTable.Name)}'.");
            }

            builder.Name = name != null ? name : builder.PropertyName;
            builder.Schema = schema;
            
            if (builder.DatabaseBuilder.Tables.Any(t => t.Schema == builder.Schema && t.Name == builder.Name))
            {
                throw new ModelBuilderException(ModelBuilderError.InvalidTableSelector, $"Table name '{BuilderHelper.GetTableName(builder.Schema, builder.Name)}' has already been used.");
            }
            
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
