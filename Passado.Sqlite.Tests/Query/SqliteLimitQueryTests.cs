using System;
using System.Collections.Generic;
using System.Text;

using Passado.Tests.Query;

namespace Passado.Sqlite.Tests.Query
{
    public class SqliteLimitQueryTests : LimitQueryTests
    {
        public override IQueryBuilder<TDatabase> GetQueryBuilder<TDatabase>() => SqliteHelpers.GetQueryBuilder<TDatabase>();
    }
}
