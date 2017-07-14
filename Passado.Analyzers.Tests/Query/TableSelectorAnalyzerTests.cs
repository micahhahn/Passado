using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

using Passado.Analyzers.Query;

namespace Passado.Analyzers.Tests
{
    public class TableSelectorAnalyzerTests
    {
        static readonly DiagnosticAnalyzer _analyzer = new QueryAnalyzerDispatcher();

        static string GenerateCode(string queryBuilder)
        {
            return @"
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
        }

        [Fact]
        public async void Error_On_Non_Simple_Lambda_Expression()
        {
            var source = GenerateCode(@"
                var dummyUsers = new List<User>();
                queryBuilder.Select(q => q.From((Database t) => dummyUsers)
                                          .Select(t => new { Name = t.T1.FirstName }));");

            var queryAnalyzer = new QueryAnalyzerDispatcher();

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(queryAnalyzer, source);
            
            Assert.Equal(1, diagnostics.Count(d => d.Id == "PassadoTableSelectorAnalyzer"));
        }
    }
}
