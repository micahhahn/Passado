using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Passado.Model;
using Passado.Model.Table;

namespace Passado.Query.Internal
{
    public abstract class ScalarSelectQueryBase : QueryBase
    {
        public ScalarSelectQueryBase(QueryBuilderBase queryBuilderBase, LambdaExpression selector)
        {
            InnerQuery = null;
            QueryBuilderBase = queryBuilderBase;
            Selector = selector;
            Columns = SelectQueryBase.GetColumns(selector);
        }

        public ImmutableArray<PropertyModel> Columns { get; }
        public LambdaExpression Selector { get; }
        public QueryBuilderBase QueryBuilderBase { get; }
    }

    // Select query without from clause (or in case this is not supported then the DUAL table is specified
    // in the from clause which only has one row)
    public class ScalarSelectQuery<TResult>
        : ScalarSelectQueryBase
        , Select.ISelectQuery<TResult>
    {
        public ScalarSelectQuery(QueryBuilderBase queryBuilderBase, LambdaExpression selector)
            : base(queryBuilderBase, selector)
        { }
    }
}
