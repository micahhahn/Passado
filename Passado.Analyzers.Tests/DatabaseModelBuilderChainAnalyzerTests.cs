using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Passado.Analyzers;

using Xunit;

namespace Passado.Analyzers.Tests
{
    public class DatabaseModelBuilderChainAnalyzerTests
    {
        private readonly static DatabaseModelBuilderChainAnalyzer _analyzer = new DatabaseModelBuilderChainAnalyzer();

        async Task<IEnumerable<(string, Diagnostic)>> RunQueryBuilderDiagnostics(string queryBuilder)
        {
            var source = @"
                using System.Collections.Generic;
                using Passado.Core;
                using Passado.Core.Model;
                using Passado.Core.Model.Builder;
                using System.Linq.Expressions;

                public class User
                {
                    public int UserId { get; set; }
                    public string FirstName { get; set; }

                    public static TableModel ProvideModel(TableModelBuilder<Database> modelBuilder)
                    {
                        return modelBuilder.Table(d => d.Users)
                                           .Column(t => t.UserId, SqlType.Int, identity: true)
                                           .Column(t => t.FirstName, SqlType.String)
                                           .PrimaryKey(t => t.UserId)
                                           .Build();
                    }
                }

                public class Database
                {
                    public IEnumerable<User> Users { get; set; }

                    [DatabaseModelProvider]
                    public static DatabaseModel ProvideModel(DatabaseModelBuilder<Database> modelBuilder)
                    {
                        return modelBuilder.Database(nameof(Database))
                                           .Table(m => m.Table(d => d.Users, name: ""Users!"")
                                                        .Column(t => t.UserId, SqlType.Int, identity: true)
                                                        .Column(t => t.FirstName, SqlType.String)
                                                        .PrimaryKey(t => t.UserId)
                                                        .Build())
                                           .Build();
                    }
                }
                ";

            var diagnostic = await CodeAnalyzerHelper.GetDiagnosticsAsync(_analyzer, source);

            return diagnostic.Select(d => (source.Substring(d.Location.SourceSpan.Start, d.Location.SourceSpan.Length), d));
        }

        [Fact]
        public async void ModelBuilderTests()
        {
            var diagnostics = await RunQueryBuilderDiagnostics("");

            Assert.Equal(1, diagnostics.Count());
        }
    }
}
