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
        private readonly ModelAnalyzer _analyzer = new ModelAnalyzer();

        async Task<IEnumerable<(string, Diagnostic)>> RunModelDiagnostics(string modelBuilder)
        {
            var source = @"
                using System;
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

                    public bool BoolColumn { get; set; }
                    public byte ByteColumn { get; set; }
                    public short ShortColumn { get; set; }
                    public int IntColumn { get; set; }
                    public long LongColumn { get; set; }
                    public float FloatColumn { get; set; }
                    public double DoubleColumn { get; set; }
                    public DateTime DateTimeColumn { get; set; }
                    public DateTimeOffset DateTimeOffsetColumn { get; set; }
                    public string StringColumn { get; set; }
                    public Guid GuidColumn { get; set; }
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
            Assert.Equal(ModelAnalyzer.InvalidTableSelector, d.Id);
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
            Assert.Equal(ModelAnalyzer.RepeatedTableSelector, d.Id);
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
            Assert.Equal(ModelAnalyzer.RepeatedTableName, d.Id);
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
            Assert.Equal(ModelAnalyzer.InvalidColumnSelector, d.Id);
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
            Assert.Equal(ModelAnalyzer.RepeatedColumnSelector, d.Id);
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
            Assert.Equal(ModelAnalyzer.RepeatedColumnName, d.Id);
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
            Assert.Equal(ModelAnalyzer.InvalidSqlType, d.Id);
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
        public async void Diagnostic_On_Type_Size_Smaller_Than_Enum_Size(string sqlType)
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
            Assert.Equal(ModelAnalyzer.InvalidSqlType, d.Id);
        }

        [Theory]
        [InlineData("t => t.StringColumn", "SqlType.String")]
        [InlineData("t => t.BoolColumn", "SqlType.Bit")]
        [InlineData("t => t.DateTimeColumn", "SqlType.DateTime")]
        [InlineData("t => t.DateTimeColumn", "SqlType.Date")]
        [InlineData("t => t.DateTimeColumn", "SqlType.Time")]
        [InlineData("t => t.DateTimeOffsetColumn", "SqlType.DateTimeOffset")]
        [InlineData("t => t.DoubleColumn", "SqlType.Double")]
        [InlineData("t => t.FloatColumn", "SqlType.Single")]
        [InlineData("t => t.GuidColumn", "SqlType.Guid")]
        public async void Diagnostic_On_Non_Numeric_Identity_Type(string selector, string sqlType)
        {
            var mb = @"return mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users)
                                             .Column(t => t.UserId, SqlType.Int)
                                             .Column(" + selector + ", " + sqlType + @", identity: true)
                                             .PrimaryKey(t => t.UserId)
                                             .Build())
                                .Build();";
            
            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(1, diagnostics.Count());

            (var e, var d) = diagnostics.First();

            Assert.Equal("identity: true", e);
            Assert.Equal(ModelAnalyzer.InvalidSqlTypeForIdentity, d.Id);
        }

        [Theory]
        [InlineData("t => t.UserId")]
        [InlineData("t => (Asc)t.UserId")]
        [InlineData("t => (Desc)t.UserId")]
        [InlineData("t => new { t.UserId }")]
        [InlineData("t => new { A = (Asc)t.UserId }")]
        [InlineData("t => new { A = (Desc)t.UserId }")]
        [InlineData("t => new { A = t.UserId, B = t.FirstName }")]
        [InlineData("t => new { A = (Asc)t.UserId, B = t.FirstName }")]
        [InlineData("t => new { A = (Desc)t.UserId, B = t.FirstName }")]
        [InlineData("t => new { A = t.UserId, B = (Asc)t.FirstName }")]
        [InlineData("t => new { A = (Asc)t.UserId, B = (Asc)t.FirstName }")]
        [InlineData("t => new { A = (Desc)t.UserId, B = (Asc)t.FirstName }")]
        [InlineData("t => new { A = t.UserId, B = (Desc)t.FirstName }")]
        [InlineData("t => new { A = (Asc)t.UserId, B = (Desc)t.FirstName }")]
        [InlineData("t => new { A = (Desc)t.UserId, B = (Desc)t.FirstName }")]
        public async void No_Diagnostic_On_Valid_PrimaryKey(string selector)
        {
            var mb = @"return mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users)
                                             .Column(t => t.UserId, SqlType.Int)
                                             .Column(t => t.FirstName, SqlType.String)
                                             .PrimaryKey(" + selector + @")
                                             .Build())
                                .Build();";

            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(0, diagnostics.Count());
        }
        
        [Theory]
        [InlineData("int", "t => (int)t.UserId")]
        public async void Diagnostic_On_PrimaryKey_Cast_Not_Asc_Or_Desc(string error, string selector)
        {
            var mb = @"return mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users)
                                             .Column(t => t.UserId, SqlType.Int)
                                             .Column(t => t.FirstName, SqlType.String)
                                             .PrimaryKey(" + selector + @")
                                             .Build())
                                .Build();";

            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(1, diagnostics.Count());

            (var e, var d) = diagnostics.First();

            Assert.Equal(error, e);
            Assert.Equal(ModelAnalyzer.InvalidOrderedSelectorCastType, d.Id);
        }
        
        [Fact]
        public async void Diagnostic_On_Index_Identical_To_PrimaryKey()
        {
            Assert.True(false);
        }
        
        [Fact]
        public async void Diagnostic_On_Multiple_Clustered_Indicies()
        {
            Assert.True(false);
        }

        [Fact]
        public async void Diagnostic_On_Repeated_Index_Name()
        {
            Assert.True(false);
        }
    }
}
