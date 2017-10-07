using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Reflection;

using Microsoft.Data.Sqlite;

using Passado.Internal;
using Passado.Query.Internal;
using Passado.Database;

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
            var a = ParseQuery(query);
            (var queryText, var parameters) = AdoHelpers.CreateNamedParameters(a.Clauses, a.Parameters);

            return new SqliteQuery(_connection, queryText, parameters);
        }
        
        public override IQuery<TResult> Build<TResult>(QueryBase query)
        {
            var a = ParseQuery(query);
            (var queryText, var parameters) = AdoHelpers.CreateNamedParameters(a.Clauses, a.Parameters);

            var selector = AdoHelpers.BuildSelector<TResult>(query);
            
            return new SqliteQuery<TResult>(_connection, queryText, selector, parameters);
        }
    }
}
