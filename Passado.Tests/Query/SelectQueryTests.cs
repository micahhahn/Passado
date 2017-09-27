using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Xunit;

using Passado.Model;
using Passado.Model.Database;

namespace Passado.Tests.Query
{
    public class EmptyDatabase
    {
        [DatabaseModelProvider]
        public static DatabaseModel ProvideModel(DatabaseBuilder<EmptyDatabase> databaseBuilder)
        {
            return databaseBuilder.Database(nameof(EmptyDatabase))
                                  .Build();
        }
    }

    public abstract class SelectQueryTests
    {
        public abstract IQueryBuilder<TDatabase> GetQueryBuilder<TDatabase>();

        [Fact]
        public void Closure_Should_Evaluate_Current_Variable_Value()
        {
            int i = 0;

            var qb = GetQueryBuilder<EmptyDatabase>();
            var query = qb.Select(t => new { I = i })
                          .Build();

            while (i < 5)
            {
                var rows = query.Execute();

                Assert.Equal(1, rows.Count());
                Assert.Equal(i, rows.Single().I);

                i++;
            }
        }
    }
}
