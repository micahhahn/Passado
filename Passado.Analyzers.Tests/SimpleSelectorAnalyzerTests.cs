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
    public class SimpleSelectorAnalyzerTests
    {
        private readonly static SimpleSelectorAnalyzer _analyzer = new SimpleSelectorAnalyzer();

        async Task<ImmutableArray<Diagnostic>> RunModelBuilderDiagnostics(string modelBuilder)
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

                    public static TableModel ProvideModel(TableModelBuilder<Database> modelBuilder)
                    {
                        " + modelBuilder + @"
                    }
                }

                public class Database
                {
                    public IEnumerable<User> Users { get; set; }
                }
                ";

            return await CodeAnalyzerHelper.GetDiagnosticsAsync(_analyzer, source);
        }
        
        async Task<ImmutableArray<Diagnostic>> RunQueryBuilderDiagnostics(string queryBuilder)
        {
            var source = @"
                using System.Collections.Generic;
                using Passado.Core;
                using System.Linq.Expressions;

                public class User
                {
                    public int UserId { get; set; }

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

            return await CodeAnalyzerHelper.GetDiagnosticsAsync(_analyzer, source);
        }

        [Theory]
        [InlineData("Table", @"var users = new List<User>();
                               return modelBuilder.Table(t => users)
                                                  .Column(t => t.UserId, SqlType.Int, identity: true)
                                                  .PrimaryKey(t => t.UserId)
                                                  .Build();")]
        [InlineData("Column", @"var users = new List<User>();
                                return modelBuilder.Table(t => t.Users)
                                                   .Column(t => users, SqlType.Int, identity: true)
                                                   .PrimaryKey(t => t.UserId)
                                                   .Build();")]
        public async void TableModelBuilder_Error_Diagnostic_On_Table_Selector_Invalid(string methodName, string modelBuilder)
        {
            var diagnostics = await RunModelBuilderDiagnostics(modelBuilder);
            
            Assert.DoesNotContain(diagnostics, d => d.Id != SimpleSelectorAnalyzer.Id);
            Assert.NotEqual(0, diagnostics.Count());
        }

        [Theory]
        [InlineData(@"qb.From(t => users);")]
        [InlineData(@"qb.Insert(t => users);")]
        [InlineData(@"qb.Update(t => users);")]
        [InlineData(@"qb.Delete(t => users);")]
        [InlineData(@"qb.From(t => t.Users)
                        .Join(t => users);")]
        [InlineData(@"qb.Update(t => t.Users)
                        .Join(t => users);")]
        [InlineData(@"qb.Delete(t => t.Users)
                        .Join(t => users);")]
        [InlineData(@"qb.From(t => t.Users)
                        .LeftJoin(t => users);")]
        [InlineData(@"qb.Update(t => t.Users)
                        .LeftJoin(t => users);")]
        [InlineData(@"qb.Delete(t => t.Users)
                        .LeftJoin(t => users);")]
        [InlineData(@"qb.From(t => t.Users)
                        .RightJoin(t => users);")]
        [InlineData(@"qb.From(t => t.Users)
                        .OuterJoin(t => users);")]
        [InlineData(@"qb.From(t => t.Users)
                        .OuterJoin(t => users);")]
        [InlineData(@"qb.From(t => t.Users)
                        .GroupBy(t => userId);")]
        [InlineData(@"qb.From(t => t.Users)
                        .GroupBy(t => t.T1.UserId, t => userId);")]
        [InlineData(@"qb.Update(t => t.Users)
                        .Set(t => userId, 7);")]
        public async void QueryBuilder_Error_Diagnostic_On_Table_Selector_Invalid(string queryBuilder)
        {
            var diagnostics = await RunQueryBuilderDiagnostics(queryBuilder);

            Assert.DoesNotContain(diagnostics, d => d.Id != SimpleSelectorAnalyzer.Id);
            Assert.NotEqual(0, diagnostics.Count());
        }
    }
}
