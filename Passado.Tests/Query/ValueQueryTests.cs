using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Xunit;

using Passado.Model;
using Passado.Model.Database;

namespace Passado.Tests.Query
{
    public abstract class ValueQueryTests
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
        public void Multiple_Values_Queries_Should_Work()
        {
            var qb = GetQueryBuilder<SimpleDatabase>();

            var query = qb.Insert(d => d.Users, t => new { t.UserId })
                          .Value(() => new User() { UserId = 1 })
                          .Value(() => new User() { UserId = 2 })
                          .Build();

            var rowsAffected = query.Execute();

            Assert.Equal(2, rowsAffected);
        }
    }
}
