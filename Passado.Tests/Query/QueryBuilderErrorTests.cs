using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Passado.Error;

namespace Passado.Tests.Query
{
    public abstract class QueryBuilderErrorTests : BuilderErrorTestsBase
    {
        public async Task VerifyErrorRaised(string qb, BuilderError builderError, params string[] locations)
        {

        }
    }
}
