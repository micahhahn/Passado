using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Data;

using Microsoft.Data.Sqlite;

namespace Passado.Sqlite
{
    public abstract class SqliteQueryBase
    {
        private readonly string _queryText;
        private readonly SqliteConnection _connection;
        private readonly ImmutableArray<(string VariableName, Func<object> ValueGetter)> _variableGetters;

        protected SqliteQueryBase(SqliteConnection connection, string queryText, ImmutableArray<(string, Func<object>)> variableGetters)
        {
            _queryText = queryText;
            _connection = connection;
            _connection.Open();
            _variableGetters = variableGetters;
        }

        protected SqliteCommand CreateCommand()
        {
            var command = _connection.CreateCommand();
            command.CommandText = _queryText;

            foreach (var pair in _variableGetters)
            {
                command.Parameters.AddWithValue(pair.VariableName, pair.ValueGetter());
            }

            return command;
        }
    }

    public class SqliteQuery : SqliteQueryBase, IQuery
    {
        public SqliteQuery(SqliteConnection connection, string queryText, ImmutableArray<(string, Func<object>)> variableGetters)
            : base(connection, queryText, variableGetters)
        { }

        public int Execute()
        {
            var command = CreateCommand();

            return command.ExecuteNonQuery();
        }

        public async Task<int> ExecuteAsync()
        {
            var command = CreateCommand();

            return await command.ExecuteNonQueryAsync();
        }
    }

    public class SqliteQuery<TResult> : SqliteQueryBase, IQuery<TResult>
    {
        private readonly Func<IDataRecord, TResult> _selector;

        public SqliteQuery(SqliteConnection connection, string queryText, Func<IDataRecord, TResult> selector, ImmutableArray<(string, Func<object>)> variableGetters)
            : base(connection, queryText, variableGetters)
        {
            _selector = selector;
        }

        public IEnumerable<TResult> Execute()
        {
            var command = CreateCommand();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    yield return _selector(reader);
                }
            }
        }

        public async Task<IEnumerable<TResult>> ExecuteAsync()
        {
            var command = CreateCommand();

            using (var reader = await command.ExecuteReaderAsync())
            {
                IEnumerable<TResult> Enumerator()
                {
                    while (reader.Read())
                    {
                        yield return _selector(reader);
                    }
                }

                return Enumerator();
            }
        }
    }
}
