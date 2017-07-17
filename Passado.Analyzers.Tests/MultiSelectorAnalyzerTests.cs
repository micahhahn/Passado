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
    public class MultiSelectorAnalyzerTests
    {
        private readonly static MultiSelectorAnalyzer _analyzer = new MultiSelectorAnalyzer();

        async Task<IEnumerable<(string, Diagnostic)>> RunQueryBuilderDiagnostics(string queryBuilder)
        {
            var source = @"
                using System.Collections.Generic;
                using Passado.Core;
                using System.Linq.Expressions;

                public class User
                {
                    public int UserId { get; set; }
                    public string FirstName { get; set; }

                    public void Query(IQueryBuilder<Database> qb)
                    {
                        var users = new List<User>();
                        var userId = 7;
                        " + queryBuilder + @"
                    }
                }

                public class Database
                {
                    public IEnumerable<User> Users { get; set; }
                }
                ";

            var diagnostic = await CodeAnalyzerHelper.GetDiagnosticsAsync(_analyzer, source);

            return diagnostic.Select(d => (source.Substring(d.Location.SourceSpan.Start, d.Location.SourceSpan.Length), d));
        }

        [Theory]
        [InlineData(@"qb.Insert(t => t.Users, t => new { userId });")]
        [InlineData(@"qb.Insert(t => t.Users, t => new { t.UserId, userId });")]
        [InlineData(@"qb.Insert(t => t.Users, t => userId);")]
        public async void QueryBuilder_Error_Diagnostic_On_Multi_Selector_Invalid(string queryBuilder)
        {
            var diagnostics = await RunQueryBuilderDiagnostics(queryBuilder);

            Assert.Equal(1, diagnostics.Count());

            (var e, var d) = diagnostics.First();

            Assert.Equal("userId", e);
            Assert.Equal(MultiSelectorAnalyzer.Id, d.Id);
        }

        [Theory]
        [InlineData(@"qb.Insert(t => t.Users, t => t.UserId);")]
        [InlineData(@"qb.Insert(t => t.Users, t => new { t.UserId });")]
        [InlineData(@"qb.Insert(t => t.Users, t => new { t.UserId, t.FirstName });")]
        public async void QueryBuilder_No_Error_On_Valid_Multi_Selector(string queryBuilder)
        {
            var diagnostics = await RunQueryBuilderDiagnostics(queryBuilder);

            Assert.Equal(0, diagnostics.Count());
        }
    }
}
