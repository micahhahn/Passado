using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

using Xunit;

namespace Passado.Tests.Model
{
    public abstract class DatabaseBuilderTests : ModelBuilderTests
    {
        [Theory]
        [InlineData("null")]
        [InlineData("(string)null")]
        //[InlineData("null as string")] GetConstantValue is not (currently) parsing "null as string" as a constant
        public async void Database__Error_On_Name_Null(string databaseName)
        {
            var mb = @"var _ = mb.Database(" + databaseName + @")
                                 .Table(d => d.Table(t => t.Users)
                                              .Column(t => t.UserId, SqlType.Int)
                                              .Build())
                                 .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.ArgumentNull("name"), databaseName);
        }

        [Theory]
        [InlineData("[")]
        [InlineData("]")]
        public void Database__Error_On_Name_Contains_Unescaped_Delimiter(string databaseName)
        {
            // Do once naming convention guidlines are established between database vendors
            Assert.True(TodoDisabled);
        }

        [Theory]
        [InlineData("null")]
        [InlineData("(Func<ITableBuilder<Database>, TableModel>)null")]
        public async void Table__Error_On_Null_Table_Builder(string tableBuilder)
        {
            var mb = @"var _ = mb.Database(nameof(Database))
                                 .Table(" + tableBuilder + @")
                                 .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.ArgumentNull("tableBuilder"), tableBuilder);
        }
    }

    public class DatabaseBuilderCoreTests : DatabaseBuilderTests
    {
        public override Task<CompilationError[]> GetCompilationErrors(Compilation compilation) => CoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
