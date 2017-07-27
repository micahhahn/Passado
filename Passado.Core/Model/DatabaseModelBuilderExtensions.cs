using System;
using System.Collections.Generic;
using System.Text;

using Passado.Core.Model.Database;

namespace Passado.Core.Model
{
    public static class DatabaseModelBuilderExtensions
    {
        public static IDatabase<TDatabase> Database<TDatabase>(this IDatabaseBuilder<TDatabase> @this,
                                                               string name)
        {
            var builder = @this as DatabaseBuilder<TDatabase>;
            
            throw new NotImplementedException();
        }

        public static ITable<TDatabase> Table<TDatabase>(this Database.ITableBuilder<TDatabase> @this,
                                                         Func<ITableBuilder<TDatabase>, TableModel> buildTableModel)
        {
            var builder = @this as DatabaseBuilder<TDatabase>;

            throw new NotImplementedException();
        }

        public static DatabaseModel Build<TDatabase>(this IDatabaseModelBuilder<TDatabase> @this)
        {
            var builder = @this as DatabaseBuilder<TDatabase>;

            // Check for object name uniqueness
            // Fill out foreign keys

            throw new NotImplementedException();
        }
    }
}
