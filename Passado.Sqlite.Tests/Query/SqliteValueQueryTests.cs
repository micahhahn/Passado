using System;
using System.Collections.Generic;
using System.Text;

using Passado.Tests.Query;

namespace Passado.Sqlite.Tests.Query
{
    public class SqliteValueQueryTests : ValueQueryTests
    {
        public override IQueryBuilder<TDatabase> GetQueryBuilder<TDatabase>() => SqliteHelpers.GetQueryBuilder<TDatabase>();
    }
}
