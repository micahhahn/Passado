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
        [InlineData(@".From(d => d.Users)
                      .GroupBy(t => new {{ t.T1.UserId }})
                      .Select(t => {0})",
                     "new { A = 7 }")]
        public async void Error_On_All_GroupBy_Keys_Not_Present_In_Select_Statement(string query, string location)
        {
            await VerifySelectErrorRaised(QueryBuilderError.SelectGroupByKeyNotPresentInSelect(), query, location);
        }
    }

    public class SelectQueryBuilderCoreTests : SelectQueryBuilderTests
    {
        protected override Task<CompilationError[]> GetCompilationErrors(Compilation compilation) => QueryCoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
