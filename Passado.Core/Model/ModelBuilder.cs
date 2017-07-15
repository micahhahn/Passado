using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Passado.Core.Model.Builder;

namespace Passado.Core.Model
{
    public static class ModelBuilder
    {
        public static DatabaseModel BuildModel(Type databaseType)
        {
            var methods = databaseType.GetTypeInfo()
                                      .GetMethods(BindingFlags.Public | BindingFlags.Static)
                                      .Where(m => m.GetCustomAttributes(typeof(DatabaseModelProviderAttribute)).Any())
                                      .ToList();

            if (methods.Count != 1)
                throw new ModelException($"'{databaseType.Name}' must have one public static function marked with the '{nameof(DatabaseModelProviderAttribute)}' to be used as a database.");

            var method = methods.Single();
            var modelBuilderType = typeof(DatabaseModelBuilder<>).MakeGenericType(databaseType);

            if (method.ReturnType != typeof(DatabaseModel) || method.GetParameters().FirstOrDefault()?.ParameterType != modelBuilderType)
                throw new ModelException($"The database model provider function for '{databaseType.Name}' must be of type 'Func<DatabaseModel, DatabaseModelBuilder<{databaseType.Name}>>'.");

            var modelBuilder = Activator.CreateInstance(modelBuilderType);

            try
            {
                return method.Invoke(null, new object[] { modelBuilder }) as DatabaseModel;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
