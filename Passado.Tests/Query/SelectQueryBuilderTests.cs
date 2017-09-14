using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

using Xunit;

using Passado.Error;

namespace Passado.Tests.Query
{
    public abstract class SelectQueryBuilderTests : BuilderErrorTestsBase
    {
        public async Task VerifySelectErrorRaised(BuilderError error, string groupBy, params string[] locations)
        {
            var source = @"
                using System;
                using System.Collections.Generic;
                using Passado;
                using Passado.Model;
                using Passado.Model.Database;
                using Passado.Query;
                using System.Linq.Expressions;

                public class User
                {
                    public int UserId { get; set; }
                    public int AddressId { get; set; }
                }

                public class Database
                {
                    public IEnumerable<User> Users { get; set; }

                    [DatabaseModelProvider]
                    public static DatabaseModel ProvideModel(DatabaseBuilder<Database> db)
                    {
                        return db.Database(nameof(Database))
                                 .Table(d => d.Table(t => t.Users)
                                              .Column(t => t.UserId, SqlType.Int)
                                              .Column(t => t.AddressId, SqlType.Int)
                                              .Build())
                                 .Build();
                    }

                    public static void RunQuery(IQueryBuilder<Database> qb)
                    {
                        qb" + string.Format(groupBy, locations) + @";
                    }
                }";

            await VerifySourceErrorRaised(source, error, locations);
        }

        [Theory]
        [InlineData("new { A = 7 }", @".From(d => d.Users)
                                       .GroupBy(t => new {{ t.T1.UserId }})
                                       .Select(t => {0})")]
        [InlineData("new { t.Keys.UserId }", @".From(d => d.Users)
                                               .GroupBy(t => new {{ t.T1.UserId, t.T1.AddressId }})
                                               .Select(t => {0})")]
        [InlineData("new { t.Keys.AddressId }", @".From(d => d.Users)
                                                  .GroupBy(t => new {{ t.T1.UserId, t.T1.AddressId }})
                                                  .Select(t => {0})")]
        public async void Error_On_All_GroupBy_Keys_Not_Present_In_Select_Statement(string location, string query)
        {
            await VerifySelectErrorRaised(QueryBuilderError.SelectGroupByKeyNotPresentInSelect(), query, location);
        }
    }

    public class SelectQueryBuilderCoreTests : SelectQueryBuilderTests
    {
        protected override Task<CompilationError[]> GetCompilationErrors(Compilation compilation) => QueryCoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
