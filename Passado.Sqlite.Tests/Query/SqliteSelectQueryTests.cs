﻿using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Data.Sqlite;

using Passado.Tests.Query;
using Passado.Database;

namespace Passado.Sqlite.Tests.Query
{
    public class SqliteSelectQueryTests : SelectQueryTests
    {
        public override IQueryBuilder<TDatabase> GetQueryBuilder<TDatabase>() => SqliteHelpers.GetQueryBuilder<TDatabase>();
    }
}
