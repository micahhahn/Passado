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
    public abstract class IndexBuilderTests : ModelBuilderErrorTests
    {
        public async Task VerifyIndexErrorRaised(BuilderError error, string location, string index)
        {
            var mb = @"var userId = 7;
                       mb.Database(nameof(Database))
                         .Table(d => d.Table(t => t.Users)
                                      .Column(t => t.UserId, SqlType.Int)
                                      .Column(t => t.UserType, SqlType.Int)                                
                                      " + string.Format(index, location) + @"
                                      .Build())
                         .Build();";

            await VerifyErrorRaised(mb, error, location);
        }

        #region KeyColumns

        [Theory]
        [InlineData("null", ".Index({0})")]
        public async void Error_On_KeyColumns_Null(string location, string index)
        {
            await VerifyIndexErrorRaised(BuilderError.ArgumentNull("keyColumns"), location, index);
        }

        [Theory]
        [InlineData("\"\"", ".Index(t => {0})")]
        public async void Error_On_KeyColumns_OrderedMultiSelector_Invalid(string location, string index)
        {
            await VerifyIndexErrorRaised(BuilderError.OrderedMultiSelectorInvalid("t"), location, index);
        }

        [Theory]
        [InlineData("userId", ".Index(t => new {{ {0} }})")]
        [InlineData("userId", ".Index((t) => new {{ {0} }})")]
        [InlineData("userId", ".Index(t => new {{ {0}, t.Asc.UserId }})")]
        [InlineData("userId", ".Index(t => new {{ t.Asc.UserId, {0} }})")]
        public async void Error_On_KeyColumn_OrderedSelector_Invalid(string location, string index)
        {
            await VerifyIndexErrorRaised(BuilderError.OrderedSelectorInvalid("t"), location, index);
        }

        [Theory]
        [InlineData("t.Asc.FirstName", ".Index(t => {0})")]
        public async void Error_On_KeyColumn_Not_In_Column_List(string location, string index)
        {
            await VerifyIndexErrorRaised(BuilderError.SelectorNotMappedToColumn("FirstName", "Users"), location, index);
        }

        #endregion

        #region Clustered

        [Theory]
        [InlineData("true", "PK_Users__UserType", @".PrimaryKey(t => t.Asc.UserType)
                                                    .Index(t => t.Asc.UserId, clustered: {0})")]
        [InlineData("true", "IX_Users__UserType", @".Index(t => t.Asc.UserType, clustered: true)
                                                    .Index(t => t.Asc.UserId, clustered: {0})")]
        public async void Error_On_Clustered_When_Previous_Index_Or_Primary_Key_Clustered(string location, string indexName, string index)
        {
            await VerifyIndexErrorRaised(ModelBuilderError.IndexClusteredAlreadySpecified(indexName), location, index);
        }

        #endregion

        #region IncludedColumns

        [Theory]
        [InlineData("\"\"", ".Index(t => t.Asc.UserId, includedColumns: t => {0})")]
        public async void Error_On_IncludedColumns_MutliSelector_Invalid(string location, string index)
        {
            await VerifyIndexErrorRaised(BuilderError.MultiSelectorInvalid("t"), location, index);
        }

        [Theory]
        [InlineData("userId", ".Index(t => t.Asc.UserId, includedColumns: t => new {{ {0} }})")]
        [InlineData("userId", ".Index(t => t.Asc.UserId, includedColumns: (t) => new {{ {0} }})")]
        [InlineData("userId", ".Index(t => t.Asc.UserId, includedColumns: t => new {{ {0}, t.UserType }})")]
        [InlineData("userId", ".Index(t => t.Asc.UserId, includedColumns: t => new {{ t.UserType, {0} }})")]
        public async void Error_On_IncludedColumn_Selector_Invalid(string location, string index)
        {
            await VerifyIndexErrorRaised(BuilderError.SelectorInvalid("t"), location, index);
        }

        [Theory]
        [InlineData("t.FirstName", ".Index(t => t.Asc.UserId, includedColumns: t => {0})")]
        public async void Error_On_IncludedColumn_Not_In_Column_List(string location, string index)
        {
            await VerifyIndexErrorRaised(BuilderError.SelectorNotMappedToColumn("FirstName", "Users"), location, index);
        }

        [Theory]
        [InlineData("t.UserId", ".Index(t => t.Asc.UserId, includedColumns: t => {0})")]
        public async void Error_On_IncludedColumn_Already_In_KeyColumns(string location, string index)
        {
            await VerifyIndexErrorRaised(ModelBuilderError.IndexIncludedColumnAlreadyInKeyColumns("UserId"), location, index);
        }

        #endregion
    }

    public class IndexBuilderCoreTests : IndexBuilderTests
    {
        protected override Task<CompilationError[]> GetCompilationErrors(Compilation compilation) => ModelCoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
