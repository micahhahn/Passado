using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using System.IO;

using Microsoft.Build;
using Microsoft.Build.Tasks;
using Microsoft.Build.Execution;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Xunit;

using Passado.Error;

namespace Passado.Tests.ModelBuilder
{
    public abstract class ModelBuilderErrorTests : BuilderErrorTestsBase
    {
        public async Task VerifyErrorRaised(string mb, BuilderError builderError, params string[] locations)
        {
            var source = @"
                using System;
                using System.Collections.Generic;
                using Passado.Model;
                using System.Linq.Expressions;

                public enum UserType
                {
                    Winner,
                    Loser
                }

                public class User
                {
                    public int UserId { get; set; }
                    public int AddressId { get; set; }
                    public string FirstName { get; set; }
                    public UserType UserType { get; set; }
                }

                public class Address
                {
                    public int AddressId { get; set; }
                    public int ZipCode { get; set; }
                }

                public class City
                {
                    public int CityId { get; set; }
                }

                public class Database
                {
                    public IEnumerable<User> Users { get; set; }
                    public IEnumerable<Address> Addresses { get; set; }
                    public IEnumerable<City> Cities { get; set; }

                    public static DatabaseModel ProvideModel(IDatabaseBuilder<Database> mb)
                    {
                        " + mb + @"
                        throw new NotImplementedException();
                    }
                }
                ";

            await VerifySourceErrorRaised(source, builderError, locations);
        }

        public static bool TodoDisabled = true;
    }
}
