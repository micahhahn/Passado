using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using Xunit;

namespace Passado.Analyzers.Tests
{
    public class ModelAnalyzerTests
    {
        private readonly DatabaseModelBuilderChainAnalyzer _analyzer = new DatabaseModelBuilderChainAnalyzer();

        async Task<IEnumerable<(string, Diagnostic)>> RunModelDiagnostics(string modelBuilder)
        {
            var source = @"
                using System.Collections.Generic;
                using Passado.Core;
                using Passado.Core.Model;
                using Passado.Core.Model.Builder;
                using System.Linq.Expressions;

                public enum UserGrade
                {
                    Freshman,
                    Sophomore,
                    Junior,
                    Senior
                }

                public class User
                {
                    public int UserId { get; set; }
                    public string FirstName { get; set; }
                    public UserGrade UserGrade { get; set; }
                }

                public class Address
                {
                    public int AddressId { get; set; }
                }

                public class Database
                {
                    public IEnumerable<User> Users { get; set; }
                    public IEnumerable<Address> Addresses { get; set; }

                    public static DatabaseModel ProviderModel(DatabaseModelBuilder<Database> mb)
                    {
                        var users = new List<User>();
                        var userId = 7;
                        " + modelBuilder + @"
                    }
                }
                ";

            var diagnostic = await CodeAnalyzerHelper.GetDiagnosticsAsync(_analyzer, source);

            return diagnostic.Select(d => (source.Substring(d.Location.SourceSpan.Start, d.Location.SourceSpan.Length), d));
        }

        [Fact]
        public async void Diagnostic_On_Invalid_TableSelector()
        {
            var mb = @"return mb.Database(nameof(Database))
                                .Table(d => d.Table(t => users)
                                             .Column(t => t.UserId, SqlType.Int)
                                             .PrimaryKey(t => t.UserId)
                                             .Build())
                                .Build();";

            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(1, diagnostics.Count());

            (var e, var d) = diagnostics.First();

            Assert.Equal("t => users", e);
            Assert.Equal(DatabaseModelBuilderChainAnalyzer.InvalidTableSelector, d.Id);
        }

        [Fact]
        public async void Diagnostic_On_Repeated_Table_Selector()
        {
            var mb = @"return mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users, name: ""A"")
                                             .Column(t => t.UserId, SqlType.Int)
                                             .PrimaryKey(t => t.UserId)
                                             .Build())
                                .Table(d => d.Table(t => t.Users, name: ""B"")
                                             .Column(t => t.UserId, SqlType.Int)
                                             .PrimaryKey(t => t.UserId)
                                             .Build())
                                .Build();";

            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(1, diagnostics.Count());

            (var e, var d) = diagnostics.First();

            Assert.Equal("t => t.Users", e);
            Assert.Equal(DatabaseModelBuilderChainAnalyzer.RepeatedTableSelector, d.Id);
        }

        [Theory]
        [InlineData("name: \"A\"", @"return mb.Database(nameof(Database))
                                              .Table(d => d.Table(t => t.Users, name: ""A"")
                                                           .Column(t => t.UserId, SqlType.Int)
                                                           .PrimaryKey(t => t.UserId)
                                                           .Build())
                                              .Table(d => d.Table(t => t.Addresses, name: ""A"")
                                                           .Column(t => t.AddressId, SqlType.Int)
                                                           .PrimaryKey(t => t.AddressId)
                                                           .Build())
                                              .Build();")]
        [InlineData("name: \"A\"", @"return mb.Database(nameof(Database))
                                              .Table(d => d.Table(t => t.Users, name: ""A"", schema: ""Schema"")
                                                           .Column(t => t.UserId, SqlType.Int)
                                                           .PrimaryKey(t => t.UserId)
                                                           .Build())
                                              .Table(d => d.Table(t => t.Addresses, name: ""A"", schema: ""Schema"")
                                                           .Column(t => t.AddressId, SqlType.Int)
                                                           .PrimaryKey(t => t.AddressId)
                                                           .Build())
                                              .Build();")]
        [InlineData("t => t.Addresses", @"return mb.Database(nameof(Database))
                                                   .Table(d => d.Table(t => t.Users, name: ""Addresses"")
                                                                .Column(t => t.UserId, SqlType.Int)
                                                                .PrimaryKey(t => t.UserId)
                                                                .Build())
                                                   .Table(d => d.Table(t => t.Addresses)
                                                                .Column(t => t.AddressId, SqlType.Int)
                                                                .PrimaryKey(t => t.AddressId)
                                                                .Build())
                                                   .Build();")]
        [InlineData("t => t.Addresses", @"return mb.Database(nameof(Database))
                                                   .Table(d => d.Table(t => t.Users, name: ""Addresses"", schema: ""Schema"")
                                                                .Column(t => t.UserId, SqlType.Int)
                                                                .PrimaryKey(t => t.UserId)
                                                                .Build())
                                                   .Table(d => d.Table(t => t.Addresses, schema: ""Schema"")
                                                                .Column(t => t.AddressId, SqlType.Int)
                                                                .PrimaryKey(t => t.AddressId)
                                                                .Build())
                                                   .Build();")]
        public async void Diagnostic_On_Repeated_Table_Name_And_Schema(string error, string mb)
        {
            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(1, diagnostics.Count());

            (var e, var d) = diagnostics.First();

            Assert.Equal(error, e);
            Assert.Equal(DatabaseModelBuilderChainAnalyzer.RepeatedTableName, d.Id);
        }

        [Theory]
        [InlineData(@"return mb.Database(nameof(Database))
                               .Table(d => d.Table(t => t.Users, name: ""A"", schema: ""Schema1"")
                                            .Column(t => t.UserId, SqlType.Int)
                                            .PrimaryKey(t => t.UserId)
                                            .Build())
                               .Table(d => d.Table(t => t.Addresses, name: ""A"", schema: ""Schema2"")
                                            .Column(t => t.AddressId, SqlType.Int)
                                            .PrimaryKey(t => t.AddressId)
                                            .Build())
                               .Build();")]
        [InlineData(@"return mb.Database(nameof(Database))
                               .Table(d => d.Table(t => t.Users, name: ""Addresses"", schema: ""Schema1"")
                                            .Column(t => t.UserId, SqlType.Int)
                                            .PrimaryKey(t => t.UserId)
                                            .Build())
                               .Table(d => d.Table(t => t.Addresses, schema: ""Schema2"")
                                            .Column(t => t.AddressId, SqlType.Int)
                                            .PrimaryKey(t => t.AddressId)
                                            .Build())
                               .Build();")]
        public async void No_Diagnostic_On_Repeated_Table_Name_And_Differing_Schema(string mb)
        {
            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(0, diagnostics.Count());
        }

        [Fact]
        public async void Diagnostic_On_Invalid_Column_Selector()
        {
            var mb = @"return mb.Database(nameof(Database))
                               .Table(d => d.Table(t => t.Users)
                                            .Column(t => userId, SqlType.Int)
                                            .PrimaryKey(t => t.UserId)
                                            .Build())
                               .Build();";

            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(1, diagnostics.Count());

            (var e, var d) = diagnostics.First();

            Assert.Equal("t => userId", e);
            Assert.Equal(DatabaseModelBuilderChainAnalyzer.InvalidColumnSelector, d.Id);
        }

        [Fact]
        public async void Diagnostic_On_Repeated_Column_Selector()
        {
            var mb = @"return mb.Database(nameof(Database))
                               .Table(d => d.Table(t => t.Users)
                                            .Column(t => t.UserId, SqlType.Int, name: ""A"")
                                            .Column(t => t.UserId, SqlType.Int, name: ""B"")
                                            .PrimaryKey(t => t.UserId)
                                            .Build())
                               .Build();";

            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(1, diagnostics.Count());

            (var e, var d) = diagnostics.First();

            Assert.Equal("t => t.UserId", e);
            Assert.Equal(DatabaseModelBuilderChainAnalyzer.RepeatedColumnSelector, d.Id);
        }

        [Theory]
        [InlineData("name: \"A\"", @"return mb.Database(nameof(Database))
                                              .Table(d => d.Table(t => t.Users)
                                                           .Column(t => t.UserId, SqlType.Int, name: ""A"")
                                                           .Column(t => t.FirstName, SqlType.String, name: ""A"")
                                                           .PrimaryKey(t => t.UserId)
                                                           .Build())
                                              .Build();")]
        [InlineData("t => t.FirstName", @"return mb.Database(nameof(Database))
                                                   .Table(d => d.Table(t => t.Users)
                                                                .Column(t => t.UserId, SqlType.Int, name: ""FirstName"")
                                                                .Column(t => t.FirstName, SqlType.String)
                                                                .PrimaryKey(t => t.UserId)
                                                                .Build())
                                                   .Build();")]
        public async void Diagnostic_On_Repeated_Column_Name(string error, string mb)
        {
            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(1, diagnostics.Count());

            (var e, var d) = diagnostics.First();

            Assert.Equal(error, e);
            Assert.Equal(DatabaseModelBuilderChainAnalyzer.RepeatedColumnName, d.Id);
        }

        [Theory]
        [InlineData("t => t.UserId", "SqlType.String")]
        [InlineData("t => t.UserGrade", "SqlType.DateTime")]
        public async void Diagnostic_On_Incompatible_Select_And_SqlType_Without_Converter(string selector, string type)
        {
            var mb = @"return mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users)
                                             .Column(" + selector + ", " + type + @")
                                             .PrimaryKey(t => t.UserId)
                                             .Build())
                                .Build();";

            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(1, diagnostics.Count());

            (var e, var d) = diagnostics.First();

            Assert.Equal(type, e);
            Assert.Equal(DatabaseModelBuilderChainAnalyzer.InvalidSqlType, d.Id);
        }

        [Theory]
        [InlineData("SqlType.Int")]
        [InlineData("SqlType.String")]
        public async void Enum_Types_Should_Be_Compatible_With_Strings_Or_Integers(string sqlType)
        {
            var mb = @"return mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users)
                                             .Column(t => t.UserGrade, " + sqlType + @")
                                             .PrimaryKey(t => t.UserId)
                                             .Build())
                                .Build();";

            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(0, diagnostics.Count());
        }

        [Theory]
        [InlineData("SqlType.String, maxLength: 2")]
        public async void Diagnostic_On_Field_Size_Smaller_Than_Enum_Size(string sqlType)
        {
            var mb = @"return mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users)
                                             .Column(t => t.UserGrade, " + sqlType + @")
                                             .PrimaryKey(t => t.UserId)
                                             .Build())
                                .Build();";

            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(1, diagnostics.Count());

            (var e, var d) = diagnostics.First();

            //Assert.Equal(type, e);
            Assert.Equal(DatabaseModelBuilderChainAnalyzer.InvalidSqlType, d.Id);
        }
    }
}
