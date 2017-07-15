using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core.Model.Builder
{
    public class DatabaseModelBuilder<TDatabase>
    {
        public DatabaseTableModelBuilder<TDatabase> Database(string name)
        {
            return new DatabaseTableModelBuilder<TDatabase>()
            {
                Name = name,
                Tables = new List<TableModel>()
            };
        }
    }
}
