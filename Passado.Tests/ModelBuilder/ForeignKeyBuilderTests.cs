using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

using Xunit;

using Passado.Error;

namespace Passado.Tests.ModelBuilder
{
    public abstract class ForeignKeyBuilderTests : ModelBuilderErrorTests
    {
        public async Task VerifyForeignKeyErrorRaised(BuilderError error, string foreignKey, params string[] locations)
        {
            var mb = @"var userId = 7;
                       mb.Database(nameof(Database))
                         .Table(d => d.Table(t => t.Users)
                                      .Column(t => t.UserId, SqlType.Int)
                                      .Column(t => t.UserType, SqlType.String)
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
            await VerifyForeignKeyErrorRaised(BuilderError.ArgumentNull("keyColumns"), foreignKey, location);
        }

        [Theory]
        [InlineData("\"\"", ".ForeignKey(t => {0}, t => t.Addresses, t => t.AddressId)")]
        public async void Error_On_KeyColumns_MultiSelector_Invalid(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(BuilderError.MultiSelectorInvalid("t"), foreignKey, location);
        }

        [Theory]
        [InlineData("userId", ".ForeignKey(t => new {{ {0} }}, t => t.Addresses, t => t.AddressId)")]
        [InlineData("userId", ".ForeignKey((t) => new {{ {0} }}, t => t.Addresses, t => t.AddressId)")]
        [InlineData("userId", ".ForeignKey(t => new {{ {0}, t.UserId }}, t => t.Addresses, t => t.AddressId)")]
        [InlineData("userId", ".ForeignKey(t => new {{ t.UserId, {0} }}, t => t.Addresses, t => t.AddressId)")]
        public async void Error_On_KeyColumn_OrderedSelector_Invalid(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(BuilderError.SelectorInvalid("t"), foreignKey, location);
        }

        [Theory]
        [InlineData("t.FirstName", ".ForeignKey(t => {0}, t => t.Addresses, t => t.AddressId)")]
        public async void Error_On_KeyColumn_Not_In_Column_List(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(BuilderError.SelectorNotMappedToColumn("FirstName", "Users"), foreignKey, location);
        }

        #endregion

        #region ReferenceTable

        [Theory]
        [InlineData("null", ".ForeignKey<Database, User, Address>(t => t.AddressId, {0}, t => t.AddressId)")]
        public async void Error_On_ReferenceTable_Null(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(BuilderError.ArgumentNull("referenceTable"), foreignKey, location);
        }

        [Theory]
        [InlineData("null as IEnumerable<Address>", ".ForeignKey(t => t.AddressId, t => {0}, t => t.AddressId)")]
        public async void Error_On_ReferenceTable_Selector_Invalid(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(BuilderError.SelectorInvalid("t"), foreignKey, location);
        }

        [Theory]
        [InlineData("t.Cities", ".ForeignKey(t => t.AddressId, t => {0}, t => t.CityId)")]
        public async void Error_On_ReferenceTable_Not_In_Table_List(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(BuilderError.SelectorNotMappedToTable("Cities", "Database"), foreignKey, location);
        }

        #endregion

        #region ReferenceColumns

        [Theory]
        [InlineData("null", ".ForeignKey(t => t.AddressId, t => t.Addresses, {0})")]
        public async void Error_On_ReferenceColumns_Null(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(BuilderError.ArgumentNull("referenceColumns"), foreignKey, location);
        }

        [Theory]
        [InlineData("userId", ".ForeignKey(t => t.AddressId, t => t.Addresses, t => new {{ {0} }})")]
        [InlineData("userId", ".ForeignKey(t => t.AddressId, t => t.Addresses, (t) => new {{ {0} }})")]
        [InlineData("userId", ".ForeignKey(t => t.AddressId, t => t.Addresses, t => new {{ {0}, t.AddressId }})")]
        [InlineData("userId", ".ForeignKey(t => t.AddressId, t => t.Addresses, t => new {{ t.AddressId, {0} }})")]
        public async void Error_On_ReferenceColumns_MultiSelector_Invalid(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(BuilderError.SelectorInvalid("t"), foreignKey, location);
        }

        [Theory]
        [InlineData("t.ZipCode", ".ForeignKey(t => t.AddressId, t => t.Addresses, t => {0})")]
        public async void Error_On_ReferenceColumn_Not_In_Column_List(string location, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(BuilderError.SelectorNotMappedToColumn("ZipCode", "Addresses"), foreignKey, location);
        }

        #endregion

        #region ColumnMatchingTests

        [Theory]
        [InlineData("new { t.UserId, t.UserType }", "t.AddressId", ".ForeignKey(t => {0}, t => t.Addresses, t => {1})")]
        public async void Error_On_Column_Counts_Not_Matching(string location1, string location2, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(ModelBuilderError.ForeignKeyColumnCountsDontMatch(), foreignKey, location1, location2);
        }

        [Theory]
        [InlineData("t.UserType", "t.AddressId", ".ForeignKey(t => {0}, t => t.Addresses, t => {1})")]
        public async void Error_On_Column_Types_Not_Matching(string location1, string location2, string foreignKey)
        {
            await VerifyForeignKeyErrorRaised(ModelBuilderError.ForeignKeyColumnTypesDontMatch("FK_Users__UserType__Addresses", "UserType", "String", "AddressId", "Int"), foreignKey, location1, location2);
        }

        #endregion
    }

    public class ForeignKeyBuilderCoreTests : ForeignKeyBuilderTests
    {
        protected override Task<CompilationError[]> GetCompilationErrors(Compilation compilation) => ModelCoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
