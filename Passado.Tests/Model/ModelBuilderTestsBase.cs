using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Passado.Tests.Model
{
    public abstract class ModelBuilderTestsBase
    {
        public abstract Task<List<(string ErrorId, string ErrorText)>> GetErrors(string source);
        
        async Task<List<(string ErrorId, string ErrorText)>> GetErrorsFromModelBuilder(string mb)
        {
            var source = @"
                using System;
                using System.Collections.Generic;
                using Passado.Model;

                public class User
                {
                    public int UserId { get; set; }
                    public string FirstName { get; set; }
                }

                public class Database
                {
                    public IEnumerable<User> Users { get; set; }

                    public static DatabaseModel ProvideModel(IDatabaseBuilder<Database> mb)
                    {
                        var users = new List<User>();
                        var userId = 7;
                        " + mb + @"
                        throw new NotImplementedException();
                    }
                }
                ";

            return await GetErrors(source);
        }

        [Fact]
        public async void Database_Error_On_Name_Null()
        {
            var mb = @"var _ = mb.Database(null)
                                 .Table(d => d.Table(t => t.Users)
                                              .Column(t => t.UserId, SqlType.Int)
                                              .Build())
                                 .Build();";

            var errors = await GetErrorsFromModelBuilder(mb);

            Assert.Equal(1, errors.Count);

            var error = errors.First();

            Assert.Equal(ModelBuilderError.InvalidDatabaseName.ErrorId, error.ErrorId);
        }
    }
}
