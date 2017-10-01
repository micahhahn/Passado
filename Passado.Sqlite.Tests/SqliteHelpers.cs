using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Data.Sqlite;

using Passado.Tests.Query;

namespace Passado.Sqlite.Tests
{
    public static class SqliteHelpers
    {
        public static IQueryBuilder<TDatabase> GetQueryBuilder<TDatabase>()
        {
            var connection = new SqliteConnection(@"Data Source=:memory:");
            connection.Open();

            var queryBuilder = new SqliteQueryBuilder<TDatabase>(connection);

            var createDatabase = Migration.Migration.CreateDatabase(queryBuilder.DatabaseModel);

            // Allow for the database creation query to be empty for empty databases
            if (!string.IsNullOrEmpty(createDatabase))
            {
                var command = connection.CreateCommand();

                command.CommandText = createDatabase;
                command.ExecuteNonQuery();
            }

            return queryBuilder;
        }
    }
}
