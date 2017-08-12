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
            var mb = @"var userId = 7;
                       mb.Database(nameof(Database))
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
            await VerifyPrimaryKeyErrorRaised(ModelBuilderError.ArgumentNull("keyColumns"), location, primaryKey);
        }

        [Theory]
        [InlineData("\"\"", ".PrimaryKey(t => {0})")]
        public async void Error_On_PrimaryKey_Ordered_Selector_Invalid(string location, string primaryKey)
        {
            await VerifyPrimaryKeyErrorRaised(ModelBuilderError.OrderedMultiSelectorInvalid("t"), location, primaryKey);
        }

        [Theory]
        [InlineData("userId", ".PrimaryKey(t => new {{ {0} }})")]
        [InlineData("userId", ".PrimaryKey((t) => new {{ {0} }})")]
        [InlineData("userId", ".PrimaryKey(t => new {{ {0}, t.Asc.FirstName }})")]
        [InlineData("userId", ".PrimaryKey(t => new {{ t.Asc.FirstName, {0} }})")]
        public async void Error_On_Invalid_PrimaryKey_Selector(string location, string primaryKey)
        {
            await VerifyPrimaryKeyErrorRaised(ModelBuilderError.OrderedSelectorInvalid("t"), location, primaryKey);
        }

        [Theory]
        [InlineData("t.Asc.FirstName", ".PrimaryKey(t => {0})")]
        public async void Error_On_PrimaryKey_Selector_Not_In_Column_List(string location, string primaryKey)
        {
            await VerifyPrimaryKeyErrorRaised(ModelBuilderError.SelectorNotMappedToColumn("FirstName", "Users"), location, primaryKey);
        }

        public void Error_On_PrimaryKey_Column_Type_Not_Supported()
        {

        }

        public void Error_on_PrimaryKey_Name_Already_Used_In_Database()
        {

        }
    }

    public class PrimaryKeyBuilderCoreTests : PrimaryKeyBuilderTests
    {
        public override Task<CompilationError[]> GetCompilationErrors(Compilation compilation) => CoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
