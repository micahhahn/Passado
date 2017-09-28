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

        static LambdaExpression GetSelector(QueryBase query)
        {
            if (query is SelectQueryBase selectQuery)
                return selectQuery.Selector;
            else if (query is ScalarSelectQueryBase scalarSelectQuery)
                return scalarSelectQuery.Selector;
            else
                return GetSelector(query.InnerQuery);
        }

        static Func<IDataRecord, TResult> BuildSelector<TResult>(QueryBase query)
        {
            var parameter = Expression.Parameter(typeof(IDataRecord));

            Expression ReadType(Type type, int index)
            {
                if (type == typeof(int))
                    return Expression.Call(parameter, typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt32)), Expression.Constant(index));

                throw new NotImplementedException();
            }

            Expression LiftColumn(Type type, int index)
            {
                // By this point, we should have verified that any column that might be null is a nullable type.
                // So our expression should look like either:
                // (IDataRecord d) => d.GetInt32(0)
                // (IDataRecord d) => d.IsDBNull(0) ? (int?)null : (int?)d.GetInt32(0)

                if (type.Name == "Nullable`1")
                {
                    var isNullExpression = Expression.Call(parameter, typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull)), Expression.Constant(index));
                    var ifNullExpression = Expression.Constant(null, type);
                    var ifNotNullExpression = Expression.Convert(ReadType(type.GenericTypeArguments[0], index), type);
                    return Expression.Condition(isNullExpression, ifNullExpression, ifNotNullExpression);
                }
                else
                {
                    return ReadType(type, index);
                }
            }

            var selector = GetSelector(query);

            if (selector.Body is NewExpression newExpression)
            {
                var selectors = newExpression.Constructor
                                     .GetParameters()
                                     .Select((p, i) =>
                                     {
                                         return LiftColumn(p.ParameterType, i);
                                     });

                return (Func<IDataRecord, TResult>)Expression.Lambda(Expression.New(newExpression.Constructor, selectors), parameter).Compile();
            }
            else if (selector.Body is MemberInitExpression memberInitExpression)
            {
                var bindings = memberInitExpression.Bindings
                                                   .Select((b, i) =>
                                                   {
                                                       var expression = LiftColumn((b as MemberAssignment).Expression.Type, i);
                                                       return Expression.Bind(b.Member, expression);
                                                   });

                memberInitExpression.Bindings
                                    .Select(b => (b as MemberAssignment).Expression);

                return (Func<IDataRecord, TResult>)Expression.Lambda(Expression.MemberInit(memberInitExpression.NewExpression, bindings), parameter).Compile();
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
