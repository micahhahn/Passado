using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Runtime.Loader;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Emit;

namespace Passado.Tests.Model
{
    public class ModelBuilderTests : ModelBuilderTestsBase
    {
        static readonly MetadataReference[] _references =
        {
            MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ExpressionType).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("mscorlib")).Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
            MetadataReference.CreateFromFile(typeof(IQueryBuilder<>).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Reflection")).Location)
        };

        public async override Task<List<(string ErrorId, string ErrorText)>> GetErrors(string source)
        {
            const string fileNamePrefix = "Source";
            const string projectName = "Project";

            var projectId = ProjectId.CreateNewId(debugName: projectName);

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
                .AddMetadataReferences(projectId, _references);

            var newFileName = $"{fileNamePrefix}1.cs";
            var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
            solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));

            var project = solution.GetProject(projectId);
            project = project.WithCompilationOptions(((CSharpCompilationOptions)project.CompilationOptions).WithOutputKind(OutputKind.DynamicallyLinkedLibrary)
                                                                                                           .WithWarningLevel(0));
            var compilation = await project.GetCompilationAsync();

            using (var memoryStream = new MemoryStream())
            {
                var result = compilation.Emit(memoryStream);

                if (!result.Success)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var testAssembly = AssemblyLoadContext.Default.LoadFromStream(memoryStream);

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

                            return new List<(string ErrorId, string ErrorText)>()
                            {
                                (mbException.ErrorId, mbException.Message)
                            };
                        }
                    }

                    return new List<(string ErrorId, string ErrorText)>();
                }
            }
        }
    }
}
