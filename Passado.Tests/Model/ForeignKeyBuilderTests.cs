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

        #region KeyColumns
        
        [Theory]
        [InlineData("null", ".ForeignKey({0}, t => t.Addresses, t => t.AddressId)")]
        public async void Error_On_KeyColumns_Null(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(ModelBuilderError.ArgumentNull("keyColumns"), location, foreignKey);
        }

        [Theory]
        [InlineData("\"\"", ".ForeignKey(t => {0}, t => t.Addresses, t => t.AddressId)")]
        public async void Error_On_KeyColumns_MultiSelector_Invalid(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(ModelBuilderError.MultiSelectorInvalid("t"), location, foreignKey);
        }

        [Theory]
        [InlineData("userId", ".ForeignKey(t => new {{ {0} }}, t => t.Addresses, t => t.AddressId)")]
        [InlineData("userId", ".ForeignKey((t) => new {{ {0} }}, t => t.Addresses, t => t.AddressId)")]
        [InlineData("userId", ".ForeignKey(t => new {{ {0}, t.UserId }}, t => t.Addresses, t => t.AddressId)")]
        [InlineData("userId", ".ForeignKey(t => new {{ t.UserId, {0} }}, t => t.Addresses, t => t.AddressId)")]
        public async void Error_On_KeyColumn_OrderedSelector_Invalid(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(ModelBuilderError.SelectorInvalid("t"), location, foreignKey);
        }

        [Theory]
        [InlineData("t.FirstName", ".ForeignKey(t => {0}, t => t.Addresses, t => t.AddressId)")]
        public async void Error_On_KeyColumn_Not_In_Column_List(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(ModelBuilderError.SelectorNotMappedToColumn("FirstName", "Users"), location, foreignKey);
        }

        #endregion
    }

    public class ForeignKeyBuilderCoreTests : ForeignKeyBuilderTests
    {
        public override Task<List<(string ErrorId, string ErrorText, Location Location, Location AdditionalLocation)>> GetErrorsFromCompilation(Compilation compilation) => CoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
