using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core.Tests
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
    }
    
    public class AlphaQueryBuilderTests
    {
        public void Query(IQueryBuilder<Database> queryBuilder)
        {
            queryBuilder.Delete(t => t.Users)
                        .Join(t => t.Addresses);
        }
    }
}
