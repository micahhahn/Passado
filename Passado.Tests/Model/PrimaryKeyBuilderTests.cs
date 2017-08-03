using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

using Xunit;

namespace Passado.Tests.Model
{
    public abstract class PrimaryKeyBuilderTests : ModelBuilderTests
    {
        public async Task VerifyPrimaryKeyErrorRaised(ModelBuilderError error, string location, string primaryKey)
        {
            var mb = @"mb.Database(nameof(Database))
                         .Table(d => d.Table(t => t.Users)
                                      .Column(t => t.UserId, SqlType.Int)
                                      " + string.Format(primaryKey, location) + @"
                                      .Build())
                         .Build();";

            await VerifyErrorRaised(mb, error, location);
        }

        [Theory]
        [InlineData("null", ".PrimaryKey({0})")]
        public async void Error_On_Null_PrimaryKey_Selector(string location, string primaryKey)
        {
            await VerifyPrimaryKeyErrorRaised(ModelBuilderError.PrimaryKeyNullSelector(), location, primaryKey);
        }
    }

    public class PrimaryKeyBuilderCoreTests : PrimaryKeyBuilderTests
    {
        public override Task<List<(string ErrorId, string ErrorText, Location Location, Location AdditionalLocation)>> GetErrorsFromCompilation(Compilation compilation) => CoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
