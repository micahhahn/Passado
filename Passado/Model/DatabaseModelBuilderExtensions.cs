using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Passado.Error;
using Passado.Model.Table;
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
                Name = name ?? throw BuilderError.ArgumentNull(nameof(name)).AsException(),
                Tables = new List<TableModel>()
            };
        }

        public static ITable<TDatabase> Table<TDatabase>(this Database.ITableBuilder<TDatabase> @this,
                                                         Func<ITableBuilder<TDatabase>, TableModel> table)
        {
            var builder = @this as InternalDatabaseBuilder<TDatabase>;

            if (table == null)
                throw BuilderError.ArgumentNull(nameof(table)).AsException();

            builder.Tables.Add(table(new TableBuilder<TDatabase>(builder)));

            return builder;
        }

        public static DatabaseModel Build<TDatabase>(this IDatabaseModelBuilder<TDatabase> @this)
        {
            var builder = @this as InternalDatabaseBuilder<TDatabase>;

            // TODO: Check for object name uniqueness
            
            foreach ((var table, var foreignKey) in builder.Tables.SelectMany(t => t.ForeignKeys, (t, f) => (t, f)))
            {
                foreignKey.FreezeReference(table, builder.Tables);

                foreach (var columnPair in foreignKey.KeyColumns.Zip(foreignKey.ReferenceColumns, (l, r) => (l, r)))
                {
                    if (columnPair.Item1.SqlType != columnPair.Item2.SqlType)
                        throw ModelBuilderError.ForeignKeyColumnTypesDontMatch(foreignKey.Name, columnPair.Item1.Name, columnPair.Item1.SqlType.ToString(), columnPair.Item2.Name, columnPair.Item2.SqlType.ToString()).AsException();
                }
            }

            return new DatabaseModel(name: builder.Name,
                                     tables: builder.Tables.ToImmutableArray());
        }
    }
}
