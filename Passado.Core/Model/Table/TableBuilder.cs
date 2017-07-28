using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core.Model.Table
{
    public class TableBuilder<TDatabase, TTable> : ITable<TDatabase, TTable>,
                                                   IColumn<TDatabase, TTable>,
                                                   IPrimaryKey<TDatabase, TTable>,
                                                   IIndex<TDatabase, TTable>,
                                                   IForeignKey<TDatabase, TTable>
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        public string PropertyName { get; set; }
        public List<ColumnModel> Columns { get; set; }
        public PrimaryKeyModel PrimaryKey { get; set; }
        public List<ForeignKeyModel> ForeignKeys { get; set; }
        public List<IndexModel> Indexes { get; set; }
    }
}
