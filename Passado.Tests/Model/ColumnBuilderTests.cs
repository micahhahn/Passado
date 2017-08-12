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

            await VerifyErrorRaised(mb, ModelBuilderError.ArgumentNull("column"), selector);
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
            await VerifyErrorRaised(mb, ModelBuilderError.SelectorInvalid("t"), selector);
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

        [Theory]
        [InlineData("SqlType.Binary",           ".Column(t => t.UserType, {0})")]
        [InlineData("SqlType.Bit",              ".Column(t => t.UserType, {0})")]
        [InlineData("SqlType.Boolean",          ".Column(t => t.UserType, {0})")]
        [InlineData("SqlType.Guid",             ".Column(t => t.UserType, {0})")]
        [InlineData("SqlType.Date",             ".Column(t => t.UserType, {0})")]
        [InlineData("SqlType.Time",             ".Column(t => t.UserType, {0})")]
        [InlineData("SqlType.DateTime",         ".Column(t => t.UserType, {0})")]
        [InlineData("SqlType.DateTimeOffset",   ".Column(t => t.UserType, {0})")]
        [InlineData("SqlType.Single",           ".Column(t => t.UserType, {0})")]
        [InlineData("SqlType.Double",           ".Column(t => t.UserType, {0})")]
        [InlineData("SqlType.Decimal",          ".Column(t => t.UserType, {0})")]
        public async void Error_On_Enum_Backed_Column_Not_String_Or_Integral_Type(string errorLocation, string column)
        {
            var mb = @"mb.Database(nameof(Database))
                         .Table(d => d.Table(t => t.Users)
                                      " + string.Format(column, errorLocation) + @"
                                      .Build())
                         .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.ColumnEnumNotStringOrIntegralType(), errorLocation);
        }

        [Theory]
        [InlineData("maxLength: 2", ".Column(t => t.UserType, SqlType.String, {0})")]
        public async void Error_On_Enum_Max_Length_Bigger_Than_String_Size(string errorLocation, string column)
        {
            var mb = @"mb.Database(nameof(Database))
                         .Table(d => d.Table(t => t.Users)
                                      " + string.Format(column, errorLocation) + @"
                                      .Build())
                         .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.ColumnEnumLongerThanMaxStringSize("UserType.Winner", 2), errorLocation);
        }

        [Fact]
        public void Error_On_Enum_Value_Larger_Than_Int_Capacity()
        {
            Assert.True(TodoDisabled);
        }

        [Fact]
        public void Column__Error_On_Column_Type_Not_Comparable()
        {
            Assert.True(TodoDisabled);
        }

        [Fact]
        public void Handling_Of_Flags_Enums()
        {
            Assert.True(TodoDisabled);
        }
    }

    public class ColumnBuilderCoreTests : ColumnBuilderTests
    {
        public override Task<CompilationError[]> GetCompilationErrors(Compilation compilation) => CoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
