using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Model.Database
{
    public class InternalDatabaseBuilder<TDatabase> : IDatabase<TDatabase>,
                                              ITable<TDatabase>
    {
        public InternalDatabaseBuilder()
        {
            Tables = new List<TableModel>();
        }

        public string Name { get; set; }
        public List<TableModel> Tables { get; set; }
    }
}
