using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

using Passado.Model;
using Passado.Model.Database;

namespace Passado.Tests.Query
{
    public class EmptyDatabase
    {
        [DatabaseModelProvider]
        public static DatabaseModel ProvideModel(DatabaseBuilder<EmptyDatabase> databaseBuilder)
        {
            return databaseBuilder.Database(nameof(EmptyDatabase))
                                  .Build();
        }
    }

    public abstract class SelectQueryTests
    {
        public abstract IQueryBuilder<TDatabase> GetQueryBuilder<TDatabase>();
    }
}
