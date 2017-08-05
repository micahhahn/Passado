using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

using Passado.Model;

namespace Passado.Tests
{
    public class User
    {
        public int UserId { get; set; }
        public int AddressId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    public class Address
    {
        public int AddressId { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }

    public class Database
    {
        public IEnumerable<User> Users { get; set; }
        public IEnumerable<Address> Addresses { get; set; }

        public static DatabaseModel Model(IDatabaseBuilder<Database> db)
        {
            return db.Database(nameof(Database))
                     .Table(d => d.Table(t => t.Users)
                                  .Column(t => t.UserId, SqlType.Int)
                                  .PrimaryKey(t => t.Asc.UserId)
                                  .Index(t => t.Desc.UserId)
                                  .Build())
                     .Build();
        }
    }

    public class AlphaQueryBuilderTests
    {
        public void Query(IQueryBuilder<Database> queryBuilder)
        {
            queryBuilder.Insert(t => t.Users, t => new { t.UserId, t.AddressId, t.FirstName, t.LastName, t.Age });

            var localParam = 7;

            queryBuilder.From(t => t.Users)
                        .Join(t => t.Addresses)
                        .As(t => new { U = t.T1, A = t.T2 })
                        .Where(t => t.U.FirstName == "John" && t.U.LastName == "Doe")
                        .GroupBy(t => new { t.A.Line1 } )
                        .Select(t => new { t.Keys.Line1, Count = t.Count(a => a.U.FirstName) })
                        .OrderBy(t => new { t.Asc.Line1, t.Desc.Count })
                        .Build();
        }
    }
}
