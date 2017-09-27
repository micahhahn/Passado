using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Data.Sqlite;

using Passado.Tests.Query;

namespace Passado.Sqlite.Tests.Query
{
    public class SqliteSelectQueryTests : SelectQueryTests
    {
        private readonly SqliteConnection _connection;

        public SqliteSelectQueryTests()
        {
            _connection = new SqliteConnection(@"Data Source=:memory:");
            _connection.Open();
        }

        public override IQueryBuilder<TDatabase> GetQueryBuilder<TDatabase>()
        {
            var queryBuilder = new SqliteQueryBuilder<TDatabase>(_connection);

            var createDatabase = Migration.Migration.CreateDatabase(queryBuilder.DatabaseModel);

            // Allow for the database creation query to be empty for empty databases
            if (!string.IsNullOrEmpty(createDatabase))
            {
                var command = _connection.CreateCommand();

                command.CommandText = createDatabase;
                command.ExecuteNonQuery();
            }
            
            return queryBuilder;
        }
    }
}
