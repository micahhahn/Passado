using System;
using System.Collections.Generic;
using System.Text;

using Passado.Core.Model.Database;

namespace Passado.Core.Model
{
    public static class DatabaseModelBuilderExtensions
    {
        public static Database<TDatabase> Database<TDatabase>(this IDatabaseBuilder<TDatabase> @this,
                                                              string name)
        {
            throw new NotImplementedException();
        }

        public static Table<TDatabase> Table<TDatabase>(this Passado.Core.Model.Database.ITableBuilder<TDatabase> @this,
                                                        Func<ITableBuilder<TDatabase>, TableModel> buildTableModel)
        {
            throw new NotImplementedException();
        }

        public static DatabaseModel Build<TDatabase>(this IDatabaseModelBuilder<TDatabase> @this)
        {
            throw new NotImplementedException();
        }
    }
}
