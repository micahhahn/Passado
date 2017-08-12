using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

using Xunit;

namespace Passado.Tests.Model
{
    public abstract class TableBuilderTests : ModelBuilderTests
    {
        [Theory]
        [InlineData("(Expression<Func<Database, IEnumerable<User>>>)null")]
        public async void Table__Error_On_Null_Table_Selector(string selector)
        {
            var mb = @"var _ = mb.Database(nameof(Database))
                                 .Table(d => d.Table(" + selector + @")
                                              .Column(t => t.UserId, SqlType.Int)
                                              .Build())
                                 .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.ArgumentNull("table"), selector);
        }

        [Theory]
        [InlineData("t => users")]
        public async void Table__Error_On_Table_Selector_Not_Property(string selector)
        {
            var mb = @"var users = new List<User>();
                       var _ = mb.Database(nameof(Database))
                                 .Table(d => d.Table(" + selector + @")
                                              .Column(t => t.UserId, SqlType.Int)
                                              .Build())
                                 .Build();";

            await VerifyErrorRaised(mb, ModelBuilderError.SelectorInvalid("t"), selector);
        }

        [Theory]
        [InlineData("t => t.Users", @"var _ = mb.Database(nameof(Database))
                                                .Table(d => d.Table(t => t.Users)
                                                             .Column(t => t.UserId, SqlType.Int)
                                                             .Build())
                                                .Table(d => d.Table(t => t.Users, name: ""A"")
                                                             .Column(t => t.UserId, SqlType.Int)
                                                             .Build())
                                                .Build();")]
        public async void Table__Error_On_Repeated_Table_Selector(string error, string mb)
        {
            await VerifyErrorRaised(mb, ModelBuilderError.TableRepeatedSelector("Database", "Users", "Users"), error);
        }

        [Theory]
        [InlineData("t => t.Users",
                    null,
                    "Users",
                    @"var _ = mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Addresses, name: ""Users"")
                                                .Column(t => t.AddressId, SqlType.Int)
                                                .Build())
                                .Table(d => d.Table(t => t.Users)
                                                .Column(t => t.UserId, SqlType.Int)
                                                .Build())
                                .Build();")]
        [InlineData("name: \"Users\"",
                    null,
                    "Users",
                    @"var _ = mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users)
                                            .Column(t => t.UserId, SqlType.Int)
                                            .Build())
                                .Table(d => d.Table(t => t.Addresses, name: ""Users"")
                                            .Column(t => t.AddressId, SqlType.Int)
                                            .Build())
                                .Build();")]
        [InlineData("name: \"A\"",
                    null,
                    "A",
                    @"var _ = mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users, name: ""A"")
                                            .Column(t => t.UserId, SqlType.Int)
                                            .Build())
                                .Table(d => d.Table(t => t.Addresses, name: ""A"")
                                            .Column(t => t.AddressId, SqlType.Int)
                                            .Build())
                                .Build();")]
        [InlineData("t => t.Users",
                    "schema: \"A\"",
                    "A.Users",
                    @"var _ = mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Addresses, schema: ""A"", name: ""Users"")
                                                .Column(t => t.AddressId, SqlType.Int)
                                                .Build())
                                .Table(d => d.Table(t => t.Users, schema: ""A"")
                                                .Column(t => t.UserId, SqlType.Int)
                                                .Build())
                                .Build();")]
        [InlineData("name: \"Users\"",
                    "schema: \"A\"",
                    "A.Users",
                    @"var _ = mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users, schema: ""A"")
                                            .Column(t => t.UserId, SqlType.Int)
                                            .Build())
                                .Table(d => d.Table(t => t.Addresses, schema: ""A"", name: ""Users"")
                                            .Column(t => t.AddressId, SqlType.Int)
                                            .Build())
                                .Build();")]
        [InlineData("name: \"A\"",
                    "schema: \"A\"",
                    "A.A",
                    @"var _ = mb.Database(nameof(Database))
                                .Table(d => d.Table(t => t.Users, schema: ""A"", name: ""A"")
                                            .Column(t => t.UserId, SqlType.Int)
                                            .Build())
                                .Table(d => d.Table(t => t.Addresses, schema: ""A"", name: ""A"")
                                            .Column(t => t.AddressId, SqlType.Int)
                                            .Build())
                                .Build();")]
        public async void Table__Error_On_Repeated_Table_Name(string error, string additionalLocation, string name, string mb)
        {
            await VerifyErrorRaised(mb, ModelBuilderError.TableRepeatedName(name), error, additionalLocation);
        }
    }

    public class TableBuilderCoreTests : TableBuilderTests
    {
        public override Task<CompilationError[]> GetCompilationErrors(Compilation compilation) => CoreHelpers.GetErrorsFromCompilation(compilation);
    }
}
