using System;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Text;

using Passado.Query;
using Passado.Query.Internal;

namespace Passado.Database
{
    /// <summary>
    /// Provides a set of helper methods for database connections build on ADO.net.
    /// </summary>
    public static class AdoHelpers
    {
        public static (string Query, ImmutableArray<(string Name, Func<object> Getter)>) CreateNamedParameters(ImmutableArray<SqlClause> clauses, ImmutableArray<MemberExpression> sqlParameters)
        {
            var parameters = new Dictionary<(Type ClosureType, Type DeclaringType, string FieldName), (string Name, Func<object> Getter)>();
            var takenNames = new HashSet<string>();
            var parameterNames = new List<string>();

            foreach (var memberExpression in sqlParameters)
            {
                if (parameters.TryGetValue((memberExpression.Expression?.Type, memberExpression.Member.DeclaringType, memberExpression.Member.Name), out var value))
                {
                    parameterNames.Add($"@{value.Name}");
                    takenNames.Add(value.Name);
                }
                else
                {
                    var newVariableName = memberExpression.Member.Name;
                    var index = 1;
                    while (takenNames.Contains(newVariableName))
                        newVariableName = $"{memberExpression.Member.Name}{++index}";

                    var func = (Func<object>)Expression.Lambda(Expression.Convert(memberExpression, typeof(object))).Compile();
                    parameters.Add((memberExpression.Expression?.Type, memberExpression.Member.DeclaringType, memberExpression.Member.Name), (newVariableName, func));
                    takenNames.Add(newVariableName);
                    parameterNames.Add($"@{newVariableName}");
                }
            }
            
            return (string.Format(string.Join("\n", clauses.Select(s => s.ClauseText)), parameterNames.ToArray()), parameters.Values.ToImmutableArray());
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

        /// <summary>
        /// Builds a result selector for connections that provide IDataRecord.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Func<IDataRecord, TResult> BuildSelector<TResult>(QueryBase query)
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

            Expression body = null;
            if (selector.Body is NewExpression newExpression)
            {
                // Constructors
                // Anonymous Expressions
                var constructorArgs = newExpression.Constructor
                                                   .GetParameters()
                                                   .Select((p, i) => LiftColumn(p.ParameterType, i));

                body = Expression.New(newExpression.Constructor, constructorArgs);
            }
            else if (selector.Body is MemberInitExpression memberInitExpression)
            {
                // Constructors + Initializers
                var constructorArgs = memberInitExpression.NewExpression
                                                          .Constructor
                                                          .GetParameters()
                                                          .Select((p, i) => LiftColumn(p.ParameterType, i));

                var constructorArgCount = constructorArgs.Count();

                var bindings = memberInitExpression.Bindings
                                                   .Select((b, i) =>
                                                   {
                                                       var expression = LiftColumn((b as MemberAssignment).Expression.Type, i + constructorArgCount);
                                                       return Expression.Bind(b.Member, expression);
                                                   });

                body = Expression.MemberInit(Expression.New(memberInitExpression.NewExpression.Constructor, constructorArgs), bindings);
            }
            else if (selector.Body is MethodCallExpression methodCallExpression && methodCallExpression.Object == null)
            {
                // Static constructor methods
                var methodArgs = methodCallExpression.Method
                                                     .GetParameters()
                                                     .Select((p, i) => LiftColumn(p.ParameterType, i));

                body = Expression.Call(null, methodCallExpression.Method, methodArgs);
            }

            return (Func<IDataRecord, TResult>)Expression.Lambda(body, parameter).Compile();
        }
    }
}
