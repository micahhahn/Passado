using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Xunit;

using Passado.Error;

namespace Passado.Tests
{
    public abstract class BuilderErrorTestsBase
    {
        static readonly MetadataReference[] _references;

        static BuilderErrorTestsBase()
        {
            // The unit tests can have multiple targets (.NET Core, Desktop, etc.) so we need to build against the appropiate assemblies.
            // The easiest way to do that is to use the assemblies of the currently executing Passado dll.

            var passadoAssembly = typeof(IQueryBuilder<>).GetTypeInfo().Assembly;

            var assemblyLocations = new List<string>()
            {
                passadoAssembly.Location,
                typeof(Assembly).Assembly.Location
            };

            assemblyLocations.AddRange(passadoAssembly.GetReferencedAssemblies()
                                                      .Select(n => Assembly.Load(n).Location));

            _references = assemblyLocations.Select(l => MetadataReference.CreateFromFile(l)).ToArray();
        }
        
        // Analyzer tests will apply an analyzer to the compilation... code tests will try and run the compilation
        // and listen for an exception
        protected abstract Task<CompilationError[]> GetCompilationErrors(Compilation compilation);
        
        public async Task VerifySourceErrorRaised(string source, BuilderError builderError, params string[] locations)
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

            var options = project.CompilationOptions
                                 .WithOutputKind(OutputKind.DynamicallyLinkedLibrary) as CSharpCompilationOptions;

            project = project.WithCompilationOptions(options);

            var errors = await GetCompilationErrors(await project.GetCompilationAsync());

            Assert.Equal(1, errors.Length);

            var error = errors.First();

            Assert.Equal(builderError.ErrorId, error.ErrorId);
            Assert.Equal(builderError.Message, error.ErrorText);

            if (error.Locations != null)
            {
                foreach (var pair in error.Locations.Zip(locations, (l, r) => (l, r)))
                {
                    Assert.Equal(pair.Item2, pair.Item1);
                }
            }
        }
    }
}
