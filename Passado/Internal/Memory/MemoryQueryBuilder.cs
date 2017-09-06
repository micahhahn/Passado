using System;
using System.Collections.Generic;
using System.Text;

using Passado.Query;
using Passado.Query.Internal;

namespace Passado.Internal.Memory
{
    public class MemoryQueryBuilder<TDatabase> : QueryBuilderBase, IQueryBuilder<TDatabase>
    {
        public MemoryQueryBuilder()
            : base(typeof(TDatabase))
        {

        }

        public override IQuery Build(QueryBase query)
        {
            throw new NotImplementedException();
        }

        public override IQuery<TResult> Build<TResult>(QueryBase query)
        {
            throw new NotImplementedException();
        }
    }
}
