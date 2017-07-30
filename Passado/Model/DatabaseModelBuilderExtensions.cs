using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Passado.Model.Database;

namespace Passado.Model
{
    public static class DatabaseModelBuilderExtensions
    {
        public static IDatabase<TDatabase> Database<TDatabase>(this IDatabaseBuilder<TDatabase> @this,
                                                               string name)
        {
            var builder = new InternalDatabaseBuilder<TDatabase>();

            if (string.IsNullOrWhiteSpace(name))
                throw new ModelBuilderException(ModelBuilderError.InvalidDatabaseName, "A database name cannot be null.");

            builder.Name = name;

            return builder;
        }

        public static ITable<TDatabase> Table<TDatabase>(this Database.ITableBuilder<TDatabase> @this,
                                                         Func<ITableBuilder<TDatabase>, TableModel> buildTableModel)
        {
            var builder = @this as InternalDatabaseBuilder<TDatabase>;

            builder.Tables.Add(buildTableModel(null));

            return builder;
        }

        public static DatabaseModel Build<TDatabase>(this IDatabaseModelBuilder<TDatabase> @this)
        {
            var builder = @this as InternalDatabaseBuilder<TDatabase>;

            // TODO: Check for object name uniqueness
            // TODO: Fill out foreign keys

            return new DatabaseModel(name: builder.Name,
                                     tables: builder.Tables.ToImmutableList());
        }
    }
}
