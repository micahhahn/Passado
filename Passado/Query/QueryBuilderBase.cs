using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Passado.Error;
using Passado.Model;
using Passado.Model.Database;

using Passado.Query.Internal;

namespace Passado.Query
{
    public abstract class QueryBuilderBase
    {
        public QueryBuilderBase(Type databaseType)
        {
            var methods = databaseType.GetTypeInfo()
                                      .GetMethods(BindingFlags.Public | BindingFlags.Static)
                                      .Where(m => m.GetCustomAttributes(typeof(DatabaseModelProviderAttribute)).Any())
                                      .ToList();

            if (methods.Count != 1)
                throw QueryBuilderError.DatabaseTypeNoModelProvider(databaseType.Name).AsException();

            var method = methods.Single();
            var modelBuilderType = typeof(DatabaseBuilder<>).MakeGenericType(databaseType);

            if (method.ReturnType != typeof(DatabaseModel) || method.GetParameters().FirstOrDefault()?.ParameterType != modelBuilderType)
                throw QueryBuilderError.DatabaseTypeModelProviderIncorrectType(databaseType.Name).AsException();

            var modelBuilder = Activator.CreateInstance(modelBuilderType);

            try
            {
                DatabaseModel = method.Invoke(null, new object[] { modelBuilder }) as DatabaseModel;
            }
            catch (TargetInvocationException ex)
            {
                // Exceptions thrown by the model builder code will be wrapped in a target invocation exception
                throw ex.InnerException;
            }
        }
        
        public DatabaseModel DatabaseModel { get; }

        public abstract IQuery Build(QueryBase query);
        public abstract IQuery<TResult> Build<TResult>(QueryBase query);
    }
}
