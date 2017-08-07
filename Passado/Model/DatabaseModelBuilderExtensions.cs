using System;
using System.Linq;
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
            return new InternalDatabaseBuilder<TDatabase>()
            {
                Name = name ?? throw ModelBuilderError.ArgumentNull(nameof(name)).AsException(),
                Tables = new List<TableModel>()
            };
        }

        public static ITable<TDatabase> Table<TDatabase>(this Database.ITableBuilder<TDatabase> @this,
                                                         Func<ITableBuilder<TDatabase>, TableModel> table)
        {
            var builder = @this as InternalDatabaseBuilder<TDatabase>;

            if (table == null)
                throw ModelBuilderError.ArgumentNull(nameof(table)).AsException();

            builder.Tables.Add(table(new Table.TableBuilder<TDatabase>(builder)));

            return builder;
        }

        public static DatabaseModel Build<TDatabase>(this IDatabaseModelBuilder<TDatabase> @this)
        {
            var builder = @this as InternalDatabaseBuilder<TDatabase>;

            // TODO: Check for object name uniqueness

            foreach (var foreignKey in builder.Tables.SelectMany(t => t.ForeignKeys))
            {
                foreignKey.FreezeReference(builder.Tables);
            }

            return new DatabaseModel(name: builder.Name,
                                     tables: builder.Tables.ToImmutableArray());
        }
    }
}
