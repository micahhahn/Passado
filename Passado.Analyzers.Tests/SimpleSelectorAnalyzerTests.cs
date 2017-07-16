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

                    public void Query(IQueryBuilder<Database> queryBuilder)
                    {
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
        [InlineData("From",      @"var users = new List<User>();
                                   queryBuilder.Select(q => q.From(t => users)
                                                             .Select(t => new { A = 1 }));")]
        [InlineData("Join",      @"var users = new List<User>();
                                   queryBuilder.Select(q => q.From(t => t.Users)
                                                             .Join(t => users)
                                                             .Select(t => new { A = 1 }));")]
        [InlineData("LeftJoin",  @"var users = new List<User>();
                                   queryBuilder.Select(q => q.From(t => t.Users)
                                                             .LeftJoin(t => users)
                                                             .Select(t => new { A = 1 }));")]
        [InlineData("RightJoin", @"var users = new List<User>();
                                   queryBuilder.Select(q => q.From(t => t.Users)
                                                             .RightJoin(t => users)
                                                             .Select(t => new { A = 1 }));")]
        [InlineData("OuterJoin", @"var users = new List<User>();
                                   queryBuilder.Select(q => q.From(t => t.Users)
                                                             .OuterJoin(t => users)
                                                             .Select(t => new { A = 1 }));")]
        public async void QueryBuilder_Error_Diagnostic_On_Table_Selector_Invalid(string methodName, string queryBuilder)
        {
            var diagnostics = await RunQueryBuilderDiagnostics(queryBuilder);

            Assert.DoesNotContain(diagnostics, d => d.Id != SimpleSelectorAnalyzer.Id);
            Assert.NotEqual(0, diagnostics.Count());
        }
    }
}
