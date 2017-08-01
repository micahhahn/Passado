using System;
using System.Collections.Generic;
using System.Text;

using Passado.Model.Database;

namespace Passado.Model.Table
{
    public class TableBuilder<TDatabase> : ITableBuilder<TDatabase>
    {
        public TableBuilder(InternalDatabaseBuilder<TDatabase> databaseBuilder)
        {
            DatabaseBuilder = databaseBuilder;
        }

        public InternalDatabaseBuilder<TDatabase> DatabaseBuilder { get; }
    }
}
