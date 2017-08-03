using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

using Xunit;

namespace Passado.Tests.Model
{
    public abstract class ColumnBuilderTests : ModelBuilderTests
    {
        [Theory]
        [InlineData("(Expression<Func<User, int>>)null")]
        public async void Column__Error_On_Null_Column_Selector(string selector)
        {
            var mb = @"mb.Database(nameof(Database))
                         .Table(d => d.Table(t => t.Users)
                                      .Column(" + selector + @", SqlType.Int)
                                      .Build())
                         .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.ColumnNullSelector(), selector);
        }

        [Theory]
        [InlineData("t => userId", @"var userId = 0;
                                     mb.Database(nameof(Database))
                                       .Table(d => d.Table(t => t.Users)
                                                    .Column(t => userId, SqlType.Int)
                                                    .Build())
                                       .Build();")]
        public async void Column__Error_On_Invalid_Column_Selector(string selector, string mb)
        {
            await VerifyErrorRaised(mb, ModelBuilderError.ColumnInvalidSelector("User"), selector);
        }

        [Fact]
        public async void Column__Error_On_Repeated_Column_Selector()
        {
            var selector = "t => t.UserId";
            var mb = @"mb.Database(nameof(Database))
                         .Table(d => d.Table(t => t.Users)
                                      .Column(" + selector + @", SqlType.Int, name: ""A"")
                                      .Column(" + selector + @", SqlType.Int)
                                      .Build())
                         .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.ColumnRepeatedSelector("User", "UserId", "A"), selector);
        }

        [Theory]
        [InlineData("t => t.FirstName", ".Column({0}, SqlType.String)")]
        [InlineData("name: \"FirstName\"", ".Column(t => t.FirstName, SqlType.String, {0})")]
        public async void Column__Error_On_Repeated_Column_Name(string errorLocation, string column)
        {
            var mb = @"mb.Database(nameof(Database))
                         .Table(d => d.Table(t => t.Users)
                                      .Column(t => t.UserId, SqlType.Int, name: ""FirstName"")
                                      " + string.Format(column, errorLocation) + @"
                                      .Build())
                         .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.ColumnRepeatedName("Users", "FirstName"), errorLocation);
        }

        [Theory]
        [InlineData("nullable: true", ".Column(t => t.UserId, SqlType.Int, identity: true, nullable: true)")]
        public async void Column__Error_On_Identity_Column_Nullable(string errorLocation, string column)
        {
            var mb = @"mb.Database(nameof(Database))
                         .Table(d => d.Table(t => t.Users)
                                      " + string.Format(column, errorLocation) + @"
                                      .Build())
                         .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.ColumnIdentityNullable(), errorLocation);
        }

        [Fact]
        public void Column__Error_On_Column_Type_Not_Comparable()
        {

        }
    }

    public class ColumnBuilderCoreTests : ColumnBuilderTests
    {
        public override Task<List<(string ErrorId, string ErrorText, Location Location, Location AdditionalLocation)>> GetErrorsFromCompilation(Compilation compilation) => CoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
