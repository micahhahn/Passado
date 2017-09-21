using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;

namespace Passado.Sqlite
{
    public class SqliteQuery : IQuery
    {
        private readonly string _queryText;
        private readonly SqliteConnection _connection;

        public SqliteQuery(SqliteConnection connection, string queryText)
        {
            _queryText = queryText;
            _connection = connection;
        }

        public int Execute()
        {
            var command = _connection.CreateCommand();
            command.CommandText = _queryText;
            return command.ExecuteNonQuery();
        }

        public Task<int> ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqliteQuery<TResult> : IQuery<TResult>
    {
        private readonly string _queryText;
        private readonly SqliteConnection _connection;

        public SqliteQuery(SqliteConnection connection, string queryText)
        {
            _queryText = queryText;
            _connection = connection;
            _connection.Open();
        }

        public IEnumerable<TResult> Execute()
        {
            var command = _connection.CreateCommand();
            command.CommandText = _queryText;
            var x = 0;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    //reader.
                }
            }

            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
