using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Emit;

namespace Passado.Tests.Model
{
    public class ModelBuilderTests : ModelBuilderTestsBase
    {
        public override Task<List<(string ErrorId, string ErrorText, Location Location)>> GetErrorsFromCompilation(Compilation compilation)
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

                    var databaseBuilderType = passadoAssembly.GetType("Passado.Model.Database.DatabaseBuilder`1").MakeGenericType(databaseType);

                    var databaseBuilder = Activator.CreateInstance(databaseBuilderType);

                    var method = databaseType.GetMethod("ProvideModel");

                    try
                    {
                        method.Invoke(null, new object[] { databaseBuilder });
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException is ModelBuilderException)
                        {
                            var mbException = ex.InnerException as ModelBuilderException;
                            
                            return Task.FromResult(new List<(string ErrorId, string ErrorText, Location Location)>()
                            {
                                (mbException.ErrorId, mbException.Message, null)
                            });
                        }
                    }

                    return Task.FromResult(new List<(string ErrorId, string ErrorText, Location Location)>());
                }
            }
        }
    }
}
