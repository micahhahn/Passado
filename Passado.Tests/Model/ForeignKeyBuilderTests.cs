using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

using Xunit;

namespace Passado.Tests.Model
{
    public abstract class ForeignKeyBuilderTests : ModelBuilderTests
    {
        public async Task VerifyForeignKeyErrorRaised(ModelBuilderError error, string location, string foreignKey)
        {
            var mb = @"var userId = 7;
                       mb.Database(nameof(Database))
                         .Table(d => d.Table(t => t.Users)
                                      .Column(t => t.UserId, SqlType.Int)
                                      .Column(t => t.UserType, SqlType.Int)                                
                                      " + string.Format(foreignKey, location) + @"
                                      .Build())
                         .Build();";

            await VerifyErrorRaised(mb, error, location);
        }

        [Fact]
        public void Error_For_Everything()
        {
            Assert.True(false);
        }
    }

    public class ForeignKeyBuilderCoreTests : ForeignKeyBuilderTests
    {
        public override Task<List<(string ErrorId, string ErrorText, Location Location, Location AdditionalLocation)>> GetErrorsFromCompilation(Compilation compilation) => CoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
