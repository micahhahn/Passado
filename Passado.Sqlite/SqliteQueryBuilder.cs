using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Data.Sqlite;

using Passado.Internal;
using Passado.Query.Internal;


namespace Passado.Sqlite
{
    public class SqliteQueryBuilder<TDatabase> : DatabaseQueryBuilder<TDatabase>
    {
        private readonly SqliteConnection _connection;

        public SqliteQueryBuilder(SqliteConnection connection)
        {
            _connection = connection ??
                          throw new ArgumentNullException(nameof(connection));
        }

        public override IQuery Build(QueryBase query)
        {
            var queryText = ParseQuery(query);

            return new SqliteQuery(_connection, queryText);
        }

        public override IQuery<TResult> Build<TResult>(QueryBase query)
        {
            var queryText = ParseQuery(query);

            return new SqliteQuery<TResult>(_connection, queryText);
        }

        public override string EscapeName(string name)
        {
            return $"\"{name}\"";
        }
    }
}
