using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Xunit;

using Passado.Model;
using Passado.Model.Database;

namespace Passado.Tests.Query
{
    public abstract class LimitQueryTests
    {
        public abstract IQueryBuilder<TDatabase> GetQueryBuilder<TDatabase>();
        
        class User
        {
            public int UserId { get; set; }
        }

        class SimpleDatabase
        {
            public IEnumerable<User> Users { get; set; }

            [DatabaseModelProvider]
            public static DatabaseModel ProvideModel(DatabaseBuilder<SimpleDatabase> databaseBuilder)
            {
                return databaseBuilder.Database(nameof(SimpleDatabase))
                                      .Table(d => d.Table(t => t.Users)
                                                   .Column(t => t.UserId, SqlType.Int)
                                                   .Build())
                                      .Build();
            }
        }

        [Fact]
        public void Limit_Should_Work_With_Offset()
        {
            var qb = GetQueryBuilder<SimpleDatabase>();

            qb.Insert(d => d.Users, t => new { t.UserId })
              .Value(() => new User() { UserId = 1 })
              .Value(() => new User() { UserId = 2 })
              .Value(() => new User() { UserId = 3 })
              .Execute();

            var query = qb.From(d => d.Users)
                          .Select(t => new { t.T1.UserId })
                          .Offset(() => 1)
                          .Limit(() => 1)
                          .Build();

            var rows = query.Execute();

            Assert.Equal(1, rows.Count());
            Assert.Equal(2, rows.Single().UserId);
        }
    }
}
