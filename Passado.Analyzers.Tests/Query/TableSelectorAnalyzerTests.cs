using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

using Passado.Analyzers.Query;

namespace Passado.Analyzers.Tests
{
    public class TableSelectorAnalyzerTests
    {
        static readonly DiagnosticAnalyzer _analyzer = new QueryAnalyzerDispatcher();

        async void CompileQuery(string queryBuilder)
        {
            var source = @"
                using System.Collections.Generic;
                using Passado.Core;

                class User
                {
                    public int UserID { get; set; }
                    public string FirstName { get; set; }
                    public string LastName { get; set; }
                    public int AddressID { get; set; }
                }

                class Address
                {
                    public int AddressID { get; set; }
                    public string City { get; set; }
                }

                class Database
                {
                    public IEnumerable<User> Users { get; set; }
                    public IEnumerable<Address> Addresses { get; set; }

                    static void QueryBuilderTest(IQueryBuilder<Database> queryBuilder)
                    {
                        " + queryBuilder + @"
                    }
                }";

            var queryAnalyzer = new QueryAnalyzerDispatcher();

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(queryAnalyzer, source);

            Assert.True(diagnostics.Count() > 0);
            Assert.True(diagnostics.All(d => d.Id == "PassadoTableSelectorAnalyzer"));
        }

        [Theory]
        [InlineData("From",      @"queryBuilder.Select(q => q.From((Database t) => t.Users)
                                                             .Select(t => new { Name = t.T1.FirstName }));")]
        [InlineData("Join",      @"queryBuilder.Select(q => q.From(t => t.Users)
                                                             .Join((Database t) => t.Addresses, t => t.T1.AddressID == t.T2.AddressID)
                                                             .Select(t => new { Name = t.T1.FirstName }));")]
        [InlineData("LeftJoin",  @"queryBuilder.Select(q => q.From(t => t.Users)
                                                             .LeftJoin((Database t) => t.Addresses, t => t.T1.AddressID == t.T2.AddressID)
                                                             .Select(t => new { Name = t.T1.FirstName }));")]
        [InlineData("RightJoin", @"queryBuilder.Select(q => q.From(t => t.Users)
                                                             .RightJoin((Database t) => t.Addresses, t => t.T1.AddressID == t.T2.AddressID)
                                                             .Select(t => new { Name = t.T1.FirstName }));")]
        [InlineData("OuterJoin", @"queryBuilder.Select(q => q.From(t => t.Users)
                                                             .OuterJoin((Database t) => t.Addresses, t => t.T1.AddressID == t.T2.AddressID)
                                                             .Select(t => new { Name = t.T1.FirstName }));")]
        public void Error_On_Non_Simple_Lambda_Expression(string name, string queryBuilder)
        {
            CompileQuery(queryBuilder);
        }

        [Theory]
        [InlineData("From", @"var users = new List<User>();
                              queryBuilder.Select(q => q.From(t => users)
                                                        .Select(t => new { Name = t.T1.FirstName }));")]
        public void Error_On_Non_Parameter_Member(string name, string queryBuilder)
        {
            CompileQuery(queryBuilder);
        }
    }
}
