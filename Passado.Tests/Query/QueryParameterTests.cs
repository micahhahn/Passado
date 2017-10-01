using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Xunit;

using Passado.Model;
using Passado.Model.Database;

namespace Passado.Tests.Query
{
    public abstract class QueryParameterTests
    {
        class EmptyDatabase
        {
            [DatabaseModelProvider]
            public static DatabaseModel ProvideModel(DatabaseBuilder<EmptyDatabase> databaseBuilder)
            {
                return databaseBuilder.Database(nameof(EmptyDatabase))
                                      .Build();
            }
        }

        public abstract IQueryBuilder<TDatabase> GetQueryBuilder<TDatabase>();

        [Fact]
        public void Closure_Should_Evaluate_Current_Variable_Value()
        {
            var qb = GetQueryBuilder<EmptyDatabase>();

            int i = 0;

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

        static int X = 7;

        public class EmptyClass
        {
            public static int X = 8;
        }

        [Fact]
        public void Closure_To_Static_Field_Should_Work()
        {
            var qb = GetQueryBuilder<EmptyDatabase>();

            var query = qb.Select(t => new { X1 = X, X2 = EmptyClass.X })
                          .Build();

            var rows = query.Execute();
            Assert.Equal(1, rows.Count());
            Assert.Equal(X, rows.Single().X1);
            Assert.Equal(EmptyClass.X, rows.Single().X2);
        }

        [Fact]
        public void Closure_Should_Work_If_Referenced_Twice()
        {
            var qb = GetQueryBuilder<EmptyDatabase>();

            int i = 0;

            var query = qb.Select(t => new { I1 = i, I2 = i })
                          .Build();

            var rows = query.Execute();

            Assert.Equal(1, rows.Count());
            Assert.Equal(i, rows.Single().I1);
            Assert.Equal(i, rows.Single().I2);
        }
    }
}
