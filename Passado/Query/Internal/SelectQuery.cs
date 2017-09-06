using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Reflection;

using Passado.Model;
using Passado.Model.Table;

namespace Passado.Query.Internal
{
    public abstract class SelectQueryBase : QueryBase
    {
        public ImmutableArray<ColumnModel> Columns { get; protected set; }
        public LambdaExpression Selector { get; protected set; }
    }

    public class SelectQuery<TResult>
        : SelectQueryBase
        , Select.ISelectQuery<TResult>
    {
        public SelectQuery(QueryBase innerQuery, LambdaExpression selector)
        {
            InnerQuery = innerQuery;
            Selector = selector;

            Columns = (selector.Body as NewExpression).Members
                                                      .Select(m => new ColumnModel(m.Name, new PropertyModel(m.Name, (m as PropertyInfo).PropertyType), SqlType.Int, false, false, 7))
                                                      .ToImmutableArray();
        }
    }
}
