using System;
using System.Collections.Generic;
using System.Text;

using Passado.Model.Database;

namespace Passado.Model.Table
{
    public class InternalTableBuilder<TDatabase, TTable> : ITable<TDatabase, TTable>,
                                                           IColumn<TDatabase, TTable>,
                                                           IPrimaryKey<TDatabase, TTable>,
                                                           IIndex<TDatabase, TTable>,
                                                           IForeignKey<TDatabase, TTable>
    {
        public InternalTableBuilder(InternalDatabaseBuilder<TDatabase> databaseBuilder)
        {
            DatabaseBuilder = databaseBuilder;
            Columns = new List<ColumnModel>();
            ForeignKeys = new List<ForeignKeyModel>();
            Indexes = new List<IndexModel>();
        }

        public string Name { get; set; }
        public string Schema { get; set; }
        public string PropertyName { get; set; }
        public List<ColumnModel> Columns { get; set; }
        public PrimaryKeyModel PrimaryKey { get; set; }
        public List<ForeignKeyModel> ForeignKeys { get; set; }
        public List<IndexModel> Indexes { get; set; }

        public InternalDatabaseBuilder<TDatabase> DatabaseBuilder { get; }
    }
}
