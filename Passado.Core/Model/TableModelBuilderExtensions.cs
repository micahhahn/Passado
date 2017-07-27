using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

using Passado.Core.Model.Table;

namespace Passado.Core.Model
{
    public static class TableModelBuilderExtensions
    {
        public static Table<TDatabase, TTable> Table<TDatabase, TTable>(this ITableBuilder<TDatabase> @this,
                                                                        Expression<Func<TDatabase, IEnumerable<TTable>>> tableSelector,
                                                                        string name = null,
                                                                        string schema = null)
        {
            throw new NotImplementedException();
        }

        public static Column<TDatabase, TTable> Column<TDatabase, TTable, TColumn>(this IColumnBuilder<TDatabase, TTable> @this,
                                                                                   Expression<Func<TTable, TColumn>> column,
                                                                                   SqlType type,
                                                                                   bool nullable = false,
                                                                                   int? maxLength = null,
                                                                                   string name = null,
                                                                                   TColumn defaultValue = default(TColumn),
                                                                                   bool identity = false,
                                                                                   IDatabaseTypeConverter<TColumn> converter = null)
        {
            throw new NotImplementedException();
        }

        public static PrimaryKey<TDatabase, TTable> PrimaryKey<TDatabase, TTable>(this IPrimaryKeyBuilder<TDatabase, TTable> @this,
                                                                                  Expression<Func<TTable, object>> keyColumns,
                                                                                  string name = null,
                                                                                  bool clustered = true)
        {
            throw new NotImplementedException();
        }

        public static Index<TDatabase, TTable> Index<TDatabase, TTable>(this IIndexBuilder<TDatabase, TTable> @this,
                                                                        Expression<Func<TTable, object>> keyColumns,
                                                                        bool unique = false,
                                                                        string name = null,
                                                                        bool clustered = false,
                                                                        Expression<Func<TTable, object>> includedColumns = null)
        {
            throw new NotImplementedException();
        }

        public static ForeignKey<TDatabase, TTable> ForeignKey<TDatabase, TTable, TReference>(this IForeignKeyBuilder<TDatabase, TTable> @this,
                                                                                              Expression<Func<TTable, object>> keyColumns,
                                                                                              Expression<Func<TDatabase, IEnumerable<TReference>>> referenceTable,
                                                                                              Expression<Func<TReference, object>> referenceColumns,
                                                                                              ForeignKeyAction updateAction = ForeignKeyAction.Cascade,
                                                                                              ForeignKeyAction deleteAction = ForeignKeyAction.Cascade,
                                                                                              string name = null)
        {
            throw new NotImplementedException();
        }

        public static TableModel Build<TDatabase, TTable>(this ITableModelBuilder<TDatabase, TTable> @this)
        {
            throw new NotImplementedException();
        }
    }
}
