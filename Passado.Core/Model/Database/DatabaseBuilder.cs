using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core.Model.Database
{
    public class DatabaseBuilder<TDatabase> : IDatabase<TDatabase>,
                                              ITable<TDatabase>
    {
        public string Name { get; set; }
        public List<TableModel> Tables { get; set; }
    }
}
