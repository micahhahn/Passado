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
    public abstract class ModelBuilderTestsBase
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

        public abstract Task<List<(string ErrorId, string ErrorText, Location Location, Location AdditionalLocation)>> GetErrorsFromCompilation(Compilation compilation);

        async Task<List<(string ErrorId, string ErrorText, string LocationText, string AdditionalLocationText)>> GetErrorsFromModelBuilder(string mb)
        {
            var source = @"
                using System;
                using System.Collections.Generic;
                using Passado.Model;
                using System.Linq.Expressions;

                public class User
                {
                    public int UserId { get; set; }
                    public string FirstName { get; set; }
                }

                public class Address
                {
                    public int AddressId { get; set; }
                }

                public class Database
                {
                    public IEnumerable<User> Users { get; set; }
                    public IEnumerable<Address> Addresses { get; set; }

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

            var errors = await GetErrorsFromCompilation(await project.GetCompilationAsync());

            string GetLocationText(Location location)
            {
                return location == null ? null : source.Substring(location.SourceSpan.Start, location.SourceSpan.Length);
            }

            return errors.Select(e => (e.ErrorId, e.ErrorText, GetLocationText(e.Location), GetLocationText(e.AdditionalLocation))).ToList();
        }

        public async Task VerifyErrorRaised(string mb, ModelBuilderError modelError, string locationText, string additionalLocationText = null)
        {
            var errors = await GetErrorsFromModelBuilder(mb);

            Assert.Equal(1, errors.Count);

            var error = errors.First();

            Assert.Equal(modelError.ErrorId, error.ErrorId);
            Assert.Equal(modelError.Message, error.ErrorText);

            if (error.LocationText != null)
            {
                Assert.Equal(locationText, error.LocationText);

                if (additionalLocationText != null)
                {
                    Assert.Equal(additionalLocationText, error.AdditionalLocationText);
                }
            }
        }

        public static bool TodoDisabled = true;

        [Theory]
        [InlineData("null")]
        [InlineData("(string)null")]
        //[InlineData("null as string")] GetConstantValue is not (currently) parsing "null as string" as a constant
        public async void Database__Error_On_Name_Null(string databaseName)
        {
            var mb = @"var _ = mb.Database(" + databaseName + @")
                                 .Table(d => d.Table(t => t.Users)
                                              .Column(t => t.UserId, SqlType.Int)
                                              .Build())
                                 .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.NullDatabaseName(), databaseName);
        }

        [Theory]
        [InlineData("[")]
        [InlineData("]")]
        public void Database__Error_On_Name_Contains_Unescaped_Delimiter(string databaseName)
        {
            // Do once naming convention guidlines are established between database vendors
            Assert.True(TodoDisabled);
        }

        [Theory]
        [InlineData("null")]
        [InlineData("(Func<ITableBuilder<Database>, TableModel>)null")]
        public async void Table__Error_On_Null_Table_Builder(string tableBuilder)
        {
            var mb = @"var _ = mb.Database(nameof(Database))
                                 .Table(" + tableBuilder + @")
                                 .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.NullTableBuilder(), tableBuilder);
        }

        [Theory]
        [InlineData("(Expression<Func<Database, IEnumerable<User>>>)null")]
        public async void Table__Error_On_Null_Table_Selector(string selector)
        {
            var mb = @"var _ = mb.Database(nameof(Database))
                                 .Table(d => d.Table(" + selector + @")
                                              .Column(t => t.UserId, SqlType.Int)
                                              .Build())
                                 .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.TableNullSelector(), selector);
        }

        [Theory]
        [InlineData("t => users")]
        public async void Table__Error_On_Table_Selector_Not_Property(string selector)
        {
            var mb = @"var users = new List<User>();
                       var _ = mb.Database(nameof(Database))
                                 .Table(d => d.Table(" + selector + @")
                                              .Column(t => t.UserId, SqlType.Int)
                                              .Build())
                                 .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.TableInvalidSelector("Database"), selector);
        }

        [Theory]
        [InlineData("t => t.Users", @"var _ = mb.Database(nameof(Database))
                                                .Table(d => d.Table(t => t.Users)
                                                             .Column(t => t.UserId, SqlType.Int)
                                                             .Build())
                                                .Table(d => d.Table(t => t.Users, name: ""A"")
                                                             .Column(t => t.UserId, SqlType.Int)
                                                             .Build())
                                                .Build();")]
        public async void Table__Error_On_Repeated_Table_Selector(string error, string mb)
        {
            await VerifyErrorRaised(mb, ModelBuilderError.TableRepeatedSelector("Database", "Users", "Users"), error);
        }

        [Theory]
        [InlineData("t => t.Users", 
                    null,
                    "Users",
                    @"var _ = mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Addresses, name: ""Users"")
                                                .Column(t => t.AddressId, SqlType.Int)
                                                .Build())
                                .Table(d => d.Table(t => t.Users)
                                                .Column(t => t.UserId, SqlType.Int)
                                                .Build())
                                .Build();")]
        [InlineData("name: \"Users\"",
                    null,
                    "Users",
                    @"var _ = mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users)
                                            .Column(t => t.UserId, SqlType.Int)
                                            .Build())
                                .Table(d => d.Table(t => t.Addresses, name: ""Users"")
                                            .Column(t => t.AddressId, SqlType.Int)
                                            .Build())
                                .Build();")]
        [InlineData("name: \"A\"",
                    null,
                    "A",
                    @"var _ = mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users, name: ""A"")
                                            .Column(t => t.UserId, SqlType.Int)
                                            .Build())
                                .Table(d => d.Table(t => t.Addresses, name: ""A"")
                                            .Column(t => t.AddressId, SqlType.Int)
                                            .Build())
                                .Build();")]
        [InlineData("t => t.Users",
                    "schema: \"A\"",
                    "A.Users",
                    @"var _ = mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Addresses, schema: ""A"", name: ""Users"")
                                                .Column(t => t.AddressId, SqlType.Int)
                                                .Build())
                                .Table(d => d.Table(t => t.Users, schema: ""A"")
                                                .Column(t => t.UserId, SqlType.Int)
                                                .Build())
                                .Build();")]
        [InlineData("name: \"Users\"", 
                    "schema: \"A\"",
                    "A.Users",
                    @"var _ = mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users, schema: ""A"")
                                            .Column(t => t.UserId, SqlType.Int)
                                            .Build())
                                .Table(d => d.Table(t => t.Addresses, schema: ""A"", name: ""Users"")
                                            .Column(t => t.AddressId, SqlType.Int)
                                            .Build())
                                .Build();")]
        [InlineData("name: \"A\"", 
                    "schema: \"A\"",
                    "A.A",
                    @"var _ = mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users, schema: ""A"", name: ""A"")
                                            .Column(t => t.UserId, SqlType.Int)
                                            .Build())
                                .Table(d => d.Table(t => t.Addresses, schema: ""A"", name: ""A"")
                                            .Column(t => t.AddressId, SqlType.Int)
                                            .Build())
                                .Build();")]
        public async void Table__Error_On_Repeated_Table_Name(string error, string additionalLocation, string name, string mb)
        {
            await VerifyErrorRaised(mb, ModelBuilderError.TableRepeatedName(name), error, additionalLocation);
        }

        [Theory]
        [InlineData("(Expression<Func<User, int>>)null")]
        public async void Column__Error_On_Null_Column_Selector(string selector)
        {
            var mb = @"mb.Database(nameof(Database))
                         .Table(d => d.Table(t => t.Users)
                                      .Column(" + selector + @", SqlType.Int)
                                      .Build())
                         .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.ColumnNullSelector(), selector);
        }

        [Theory]
        [InlineData("t => userId", @"var userId = 0;
                                     mb.Database(nameof(Database))
                                       .Table(d => d.Table(t => t.Users)
                                                    .Column(t => userId, SqlType.Int)
                                                    .Build())
                                       .Build();")]
        public async void Column__Error_On_Invalid_Column_Selector(string selector, string mb)
        {
            await VerifyErrorRaised(mb, ModelBuilderError.ColumnInvalidSelector("User"), selector);
        }

        [Fact]
        public async void Column__Error_On_Repeated_Column_Name()
        {
            var selector = "t => t.UserId";
            var mb = @"mb.Database(nameof(Database))
                         .Table(d => d.Table(t => t.Users)
                                      .Column(" + selector + @", SqlType.Int, name: ""A"")
                                      .Column(" + selector + @", SqlType.Int)
                                      .Build())
                         .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.ColumnRepeatedSelector("User", "UserId", "A"), selector);
        }
    }
}
