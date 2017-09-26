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
            var parsedQuery = ParseQuery(query);
            
            return new SqliteQuery(_connection, parsedQuery.QueryText, parsedQuery.Variables.Values.ToImmutableArray());
        }

        static SelectQueryBase GetSelectQuery(QueryBase query)
        {
            if (query is SelectQueryBase selectQuery)
                return selectQuery;
            else
                return GetSelectQuery(query.InnerQuery);
        }

        static Func<IDataRecord, TResult> BuildSelector<TResult>(QueryBase query)
        {
            var selectQuery = GetSelectQuery(query);

            if (selectQuery.Selector.Body is NewExpression newExpression)
            {
                var parameter = Expression.Parameter(typeof(IDataRecord));
                var selectors = newExpression.Constructor
                                     .GetParameters()
                                     .Select((p, i) =>
                                     {
                                         // By this point, we should have verified that any column that might be null is a nullable type.
                                         // So our expression should look like either:
                                         // (IDataRecord d) => d.GetInt32(0)
                                         // (IDataRecord d) => d.IsDBNull(0) ? (int?)null : (int?)d.GetInt32(0)
                                         Expression ConvertType(Type type)
                                         {
                                             if (type == typeof(int))
                                                 return Expression.Call(parameter, typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt32)), Expression.Constant(i));

                                             throw new NotImplementedException();
                                         }
                                         
                                         if (p.ParameterType.Name == "Nullable`1")
                                         {
                                             var isNullExpression = Expression.Call(parameter, typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull)), Expression.Constant(i));
                                             var ifNullExpression = Expression.Constant(null, p.ParameterType);
                                             var ifNotNullExpression = Expression.Convert(ConvertType(p.ParameterType.GenericTypeArguments[0]), p.ParameterType);
                                             return Expression.Condition(isNullExpression, ifNullExpression, ifNotNullExpression);
                                         }
                                         else
                                         {
                                             return ConvertType(p.ParameterType);
                                         }
                                     });

                return (Func<IDataRecord, TResult>)Expression.Lambda(Expression.New(newExpression.Constructor, selectors), parameter).Compile();
            }

            throw new NotImplementedException();
        }

        public override IQuery<TResult> Build<TResult>(QueryBase query)
        {
            var parsedQuery = ParseQuery(query);

            var selector = BuildSelector<TResult>(query);
            
            return new SqliteQuery<TResult>(_connection, parsedQuery.QueryText, selector, parsedQuery.Variables.Values.ToImmutableArray());
        }

        public override string EscapeName(string name)
        {
            return $"\"{name}\"";
        }
    }
}
