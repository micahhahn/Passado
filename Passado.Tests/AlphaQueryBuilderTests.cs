using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

using Passado.Query.Dynamic;
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
                                  .PrimaryKey(t => t.UserId)
                                  .Index(t => t.UserId)
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
                        .Where(t => Expression.Constant(localParam));
        }

        public void DynamicQuery(IQueryBuilder<Database> queryBuilder)
        {
            queryBuilder.From(t => t.Users)
                        .Join(t => t.Addresses, p => Expression.Constant(true));
        }
    }
}
