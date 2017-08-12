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
        public async Task VerifyForeignKeyErrorRaised(ModelBuilderError error, string foreignKey, params string[] locations)
        {
            var mb = @"var userId = 7;
                       mb.Database(nameof(Database))
                         .Table(d => d.Table(t => t.Users)
                                      .Column(t => t.UserId, SqlType.Int)
                                      .Column(t => t.UserType, SqlType.Int)
                                      .Column(t => t.AddressId, SqlType.Int)
                                      " + string.Format(foreignKey, locations) + @"
                                      .Build())
                         .Table(d => d.Table(t => t.Addresses)
                                      .Column(t => t.AddressId, SqlType.Int)
                                      .Build())
                         .Build();";

            await VerifyErrorRaised(mb, error, locations);
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

        #region ReferenceTable

        [Theory]
        [InlineData("null", ".ForeignKey<Database, User, Address>(t => t.AddressId, {0}, t => t.AddressId)")]
        public async void Error_On_ReferenceTable_Null(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(ModelBuilderError.ArgumentNull("referenceTable"), location, foreignKey);
        }

        [Theory]
        [InlineData("null as IEnumerable<Address>", ".ForeignKey(t => t.AddressId, t => {0}, t => t.AddressId)")]
        public async void Error_On_ReferenceTable_Selector_Invalid(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(ModelBuilderError.SelectorInvalid("t"), location, foreignKey);
        }

        [Theory]
        [InlineData("t.Cities", ".ForeignKey(t => t.AddressId, t => {0}, t => t.CityId)")]
        public async void Error_On_ReferenceTable_Not_In_Table_List(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(ModelBuilderError.SelectorNotMappedToTable("Cities", "Database"), location, foreignKey);
        }

        #endregion

        #region ReferenceColumns

        [Theory]
        [InlineData("null", ".ForeignKey(t => t.AddressId, t => t.Addresses, {0})")]
        public async void Error_On_ReferenceColumns_Null(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(ModelBuilderError.ArgumentNull("referenceColumns"), location, foreignKey);
        }

        [Theory]
        [InlineData("userId", ".ForeignKey(t => t.AddressId, t => t.Addresses, t => new {{ {0} }})")]
        [InlineData("userId", ".ForeignKey(t => t.AddressId, t => t.Addresses, (t) => new {{ {0} }})")]
        [InlineData("userId", ".ForeignKey(t => t.AddressId, t => t.Addresses, t => new {{ {0}, t.AddressId }})")]
        [InlineData("userId", ".ForeignKey(t => t.AddressId, t => t.Addresses, t => new {{ t.AddressId, {0} }})")]
        public async void Error_On_ReferenceColumns_MultiSelector_Invalid(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(ModelBuilderError.SelectorInvalid("t"), location, foreignKey);
        }

        [Theory]
        [InlineData("t.ZipCode", ".ForeignKey(t => t.AddressId, t => t.Addresses, t => {0})")]
        public async void Error_On_ReferenceColumn_Not_In_Column_List(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(ModelBuilderError.SelectorNotMappedToColumn("ZipCode", "Addresses"), location, foreignKey);
        }

        #endregion

        #region ColumnMatchingTests

        [Theory]
        [InlineData()]
        public async void Error_On_Column_Counts_Not_Matching(string location, string foreignKey)
        {

        }

        #endregion
    }

    public class ForeignKeyBuilderCoreTests : ForeignKeyBuilderTests
    {
        public override Task<CompilationError[]> GetCompilationErrors(Compilation compilation) => CoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
