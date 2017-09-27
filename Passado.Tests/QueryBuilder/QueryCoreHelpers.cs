using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Passado.Model;
using Passado.Query;
using Passado.Query.Internal;

using Passado.Error;

namespace Passado.Tests.QueryBuilder
{
    public static class QueryCoreHelpers
    {
        public static Task<CompilationError[]> GetErrorsFromCompilation(Compilation compilation)
        {
            using (var memoryStream = new MemoryStream())
            {
                var result = compilation.Emit(memoryStream);

                if (!result.Success)
                {
                    throw new Exception(string.Join("\n", result.Diagnostics.Select(d => d.ToString()).ToArray()));
                }
                else
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    var testAssembly = Assembly.Load(memoryStream.ToArray());

                    var passadoAssembly = typeof(IQueryBuilder<>).GetTypeInfo().Assembly;

                    var databaseType = testAssembly.GetType("Database");

                    var memoryQueryBuilderType = passadoAssembly.GetType("Passado.Internal.Memory.MemoryQueryBuilder`1").MakeGenericType(databaseType);
                    
                    var memoryQueryBuilder = Activator.CreateInstance(memoryQueryBuilderType);

                    var method = databaseType.GetMethod("RunQuery");

                    try
                    {
                        method.Invoke(null, new object[] { memoryQueryBuilder });
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException is BuilderException)
                        {
                            var mbException = ex.InnerException as BuilderException;

                            return Task.FromResult(new CompilationError[]
                            {
                                new CompilationError() { ErrorId = mbException.ErrorId, ErrorText = mbException.Message }
                            });
                        }
                    }

                    return Task.FromResult(new CompilationError[] { });
                }
            }
        }
    }
}
