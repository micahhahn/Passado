using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Passado.Model.Table;

namespace Passado.Query.Internal
{
    public abstract class GroupByQueryBase : QueryBase
    {
        public ImmutableArray<ColumnModel> KeyColumns { get; protected set; }
    }

    public class GroupByQuery<TGroupedRow>
        : GroupByQueryBase
        , Select.IGroupByQuery<TGroupedRow>
    {
        public GroupByQuery(QueryBase innerQuery, Expression keys)
        {
            InnerQuery = innerQuery;
            KeyColumns = ImmutableArray.Create<ColumnModel>();
        }
    }
}
