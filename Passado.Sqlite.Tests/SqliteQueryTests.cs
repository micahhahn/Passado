using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Data.Sqlite;

using Passado.Tests;

namespace Passado.Sqlite.Tests
{
    public class SqliteQueryTests : QueryTests
    {
        private readonly SqliteConnection _connection;

        public SqliteQueryTests()
        {
            _connection = new SqliteConnection(@"Data Source=:memory:");
            _connection.Open();
        }

        ~SqliteQueryTests()
        {
            _connection.Close();
        }

        public override IQueryBuilder<TDatabase> GetQueryBuilder<TDatabase>()
        {
            var queryBuilder = new SqliteQueryBuilder<TDatabase>(_connection);

            var createDatabase = Migration.Migration.CreateDatabase(queryBuilder.DatabaseModel);

            var command = _connection.CreateCommand();

            command.CommandText = createDatabase;
            command.ExecuteNonQuery();

            return queryBuilder;
        }
    }
}
