using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xunit;

using Passado.Model;
using Passado.Model.Database;

namespace Passado.Tests
{
    public class User
    {
        public int UserId { get; set; }
    }

    public class Database
    {
        public IEnumerable<User> Users { get; set; }

        [DatabaseModelProvider]
        public static DatabaseModel ProvideModel(DatabaseBuilder<Database> databaseBuilder)
        {
            return databaseBuilder.Database(nameof(Database))
                                  .Table(q => q.Table(d => d.Users)
                                               .Column(t => t.UserId, SqlType.Int)
                                               .Build())
                                  .Build();
        }
    }

    public abstract class QueryTests
    {
        public abstract IQueryBuilder<TDatabase> GetQueryBuilder<TDatabase>();

        [Fact]
        public void BasicTest()
        {
            var qb = GetQueryBuilder<Database>();

            var y = 7;

            qb.Insert(d => d.Users, t => new { t.UserId })
              .Value(() => new User() { UserId = y });

            var query = qb.From(d => d.Users)
                          .Select(t => new { t.T1.UserId })
                          .Build();

            Assert.Equal(query.Execute().Count(), 0);
        }
    }
}
