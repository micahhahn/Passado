using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using System.IO;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace Passado.Tests.Model
{
    public abstract class ModelBuilderTests
    {
        static readonly string frameworkPath = new FileInfo(typeof(object).GetTypeInfo().Assembly.Location).Directory.FullName;

        static readonly MetadataReference[] _references =
        {
            MetadataReference.CreateFromFile(Path.Combine(frameworkPath, "mscorlib.dll")),
            MetadataReference.CreateFromFile(Path.Combine(frameworkPath, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(frameworkPath, "System.Linq.Expressions.dll")),
            MetadataReference.CreateFromFile(Path.Combine(frameworkPath, "System.Core.dll")),
            MetadataReference.CreateFromFile(typeof(IQueryBuilder<>).GetTypeInfo().Assembly.Location),
        };

        public abstract Task<CompilationError[]> GetCompilationErrors(Compilation compilation);

        async Task<CompilationError[]> GetErrorsFromModelBuilder(string mb)
        {
            var source = @"
                using System;
                using System.Collections.Generic;
                using Passado.Model;
                using System.Linq.Expressions;

                public enum UserType
                {
                    Winner,
                    Loser
                }

                public class User
                {
                    public int UserId { get; set; }
                    public int AddressId { get; set; }
                    public string FirstName { get; set; }
                    public UserType UserType { get; set; }
                }

                public class Address
                {
                    public int AddressId { get; set; }
                    public int ZipCode { get; set; }
                }

                public class City
                {
                    public int CityId { get; set; }
                }

                public class Database
                {
                    public IEnumerable<User> Users { get; set; }
                    public IEnumerable<Address> Addresses { get; set; }
                    public IEnumerable<City> Cities { get; set; }

                    public static DatabaseModel ProvideModel(IDatabaseBuilder<Database> mb)
                    {
                        " + mb + @"
                        throw new NotImplementedException();
                    }
                }
                ";

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
                                 .WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default)
                                 .WithOutputKind(OutputKind.DynamicallyLinkedLibrary) as CSharpCompilationOptions;
            
            project = project.WithCompilationOptions(options);

            return await GetCompilationErrors(await project.GetCompilationAsync());
        }

        public async Task VerifyErrorRaised(string mb, ModelBuilderError modelError, params string[] locations)
        {
            var errors = await GetErrorsFromModelBuilder(mb);

            Assert.Equal(1, errors.Length);

            var error = errors.First();

            Assert.Equal(modelError.ErrorId, error.ErrorId);
            Assert.Equal(modelError.Message, error.ErrorText);

            if (error.Locations != null)
            {
                foreach (var pair in error.Locations.Zip(locations, (l, r) => (l, r)))
                {
                    Assert.Equal(pair.Item2, pair.Item1);
                }
            }
        }

        public static bool TodoDisabled = true;
    }
}
