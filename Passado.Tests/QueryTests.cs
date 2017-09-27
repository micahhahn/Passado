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
        public int AddressId { get; set; }
        public int? Age { get; set; }
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
                                               .Column(t => t.AddressId, SqlType.Int)
                                               .Column(t => t.Age, SqlType.Int, nullable: true)
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

            var closureName = 7;

            qb.Insert(d => d.Users, t => new { t.UserId, t.AddressId })
              .Value(() => new User() { UserId = closureName, AddressId = closureName })
              .Execute();

            qb.Select(t => new { X = 7, Y = 8 })
              .Execute();

            var query = qb.From(d => d.Users)
                          .Select(t => new { t.T1.UserId, t.T1.Age })
                          .Build();

            var _ = query.Execute().ToArray();

            Assert.Equal(_.Count(), 1);
        }
    }
}
