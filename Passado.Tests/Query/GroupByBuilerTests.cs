﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

using Xunit;

using Passado.Error;

namespace Passado.Tests.Query
{
    public abstract class GroupByBuilerTests : BuilderErrorTestsBase
    {
        public async Task VerifyGroupByErrorRaised(BuilderError error, string groupBy, params string[] locations)
        {
            var source = @"
                using System;
                using System.Collections.Generic;
                using Passado;
                using Passado.Model;
                using Passado.Model.Database;
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
                        qb.From(d => d.Users)
                          " + string.Format(groupBy, locations) + @";
                    }
                }";

            await VerifySourceErrorRaised(source, error, locations);
        }

        [Theory]
        [InlineData(".GroupBy(t => {0})", "7")]
        public async void Error_On_Keys_Not_New_Expression(string groupBy, string location)
        {
            await VerifyGroupByErrorRaised(QueryBuilderError.GroupByNotNewExpression(), groupBy, location);
        }
    }

    public class GroupByBuilderCoreTests : GroupByBuilerTests
    {
        protected override Task<CompilationError[]> GetCompilationErrors(Compilation compilation) => QueryCoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
