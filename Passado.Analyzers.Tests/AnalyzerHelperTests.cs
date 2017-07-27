using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.CodeAnalysis;

using Xunit;

namespace Passado.Analyzers.Tests
{
    public class AnalyzerHelperTests
    {
        readonly ModelAnalyzer _modelAnalyzer = new ModelAnalyzer();

        async Task<IEnumerable<(string, Diagnostic)>> RunModelDiagnostics(string modelBuilder)
        {
            var source = @"
                using System;
                using System.Collections.Generic;
                using Passado.Core;
                using Passado.Core.Model;
                using Passado.Core.Model.Builder;
                using System.Linq.Expressions;

                public class User
                {
                    public int UserId { get; set; }
                    public string FirstName { get; set; }
                }

                public class Database
                {
                    public IEnumerable<User> Users { get; set; }

                    public static DatabaseModel ProviderModel(DatabaseModelBuilder<Database> mb)
                    {
                        var users = new List<User>();
                        var userId = 7;
                        " + modelBuilder + @"
                    }
                }
                ";

            var diagnostic = await CodeAnalyzerHelper.GetDiagnosticsAsync(_modelAnalyzer, source);

            return diagnostic.Select(d => (source.Substring(d.Location.SourceSpan.Start, d.Location.SourceSpan.Length), d));
        }

        static bool TodoDisabled = true;

        #region Selector Tests

        [Theory]
        [InlineData("null")]
        [InlineData("(Expression<Func<Database, IEnumerable<User>>>)null")]
        public async void Diagnostic_On_Null_Required_Selector(string tableSelector)
        {
            var mb = @"return mb.Database(nameof(Database))
                                .Table(d => d.Table<User>(" + tableSelector + @")
                                             .Column(t => t.UserId, SqlType.Int)
                                             .PrimaryKey(t => t.UserId)
                                             .Build())
                                .Build();";

            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(1, diagnostics.Count());

            (var e, var d) = diagnostics.First();

            Assert.Equal(tableSelector, e);
            Assert.Equal(AnalyzerHelpers.InvalidSelector, d.Id);
        }

        [Fact]
        public async void No_Diagnostic_On_Null_Optional_Selector()
        {
            Assert.True(TodoDisabled);
        }

        [Fact]
        public async void No_Diagnostic_On_Parenthesized_Lambda_Selector()
        {
            var mb = @"return mb.Database(nameof(Database))
                                .Table(d => d.Table<User>(null)
                                             .Column(t => t.UserId, SqlType.Int)
                                             .PrimaryKey(t => t.UserId)
                                             .Build())
                                .Build();";

            var diagnostics = await RunModelDiagnostics(mb);

            Assert.Equal(0, diagnostics.Count());
        }

        #endregion
    }
}
