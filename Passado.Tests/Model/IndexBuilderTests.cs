using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

using Xunit;

namespace Passado.Tests.Model
{
    public abstract class IndexBuilderTests : ModelBuilderTests
    {
        public async Task VerifyIndexErrorRaised(ModelBuilderError error, string location, string index)
        {
            var mb = @"var userId = 7;
                       mb.Database(nameof(Database))
                         .Table(d => d.Table(t => t.Users)
                                      .Column(t => t.UserId, SqlType.Int)
                                      " + string.Format(index, location) + @"
                                      .Build())
                         .Build();";

            await VerifyErrorRaised(mb, error, location);
        }

        [Theory]
        [InlineData("includedColumns: t => \"asdf\"", ".Index(t => t.Asc.UserId, {0})")]
        public async void Error_On_Invalid_Included_Columns_Selector(string location, string index)
        {
            await VerifyIndexErrorRaised(ModelBuilderError.MultiSelectorInvalid("t"), location, index);
        }
    }

    public class IndexBuilderCoreTests : IndexBuilderTests
    {
        public override Task<List<(string ErrorId, string ErrorText, Location Location, Location AdditionalLocation)>> GetErrorsFromCompilation(Compilation compilation) => CoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
