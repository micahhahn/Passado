using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Immutable;
using System.Text;

using Passado.Model;
using Passado.Model.Table;

namespace Passado.Query.Internal
{
    public abstract class InsertQueryBase : QueryBase
    {
        public InsertQueryBase(QueryBuilderBase queryBuilderBase, LambdaExpression table, LambdaExpression columns)
        {
            InnerQuery = null;
            QueryBuilderBase = queryBuilderBase;

            var property = ExpressionHelpers.ParseSelector(table);
            Model = queryBuilderBase.DatabaseModel.Tables.First(t => t.Property.Name == property.Name);

            IntoColumns = (columns.Body as NewExpression).Arguments
                                                         .Select(a => Model.Columns.Single(c => c.Property.Name == (a as MemberExpression).Member.Name))
                                                         .ToImmutableArray();
        }

        public QueryBuilderBase QueryBuilderBase { get; }
        public TableModel Model { get; }
        public ImmutableArray<ColumnModel> IntoColumns { get; }
    }

    public class InsertQuery<TDatabase, TIntoTable>
        : InsertQueryBase
        , Insert.IInsertQuery<TDatabase, TIntoTable>
    {
        public InsertQuery(QueryBuilderBase queryBuilderBase, LambdaExpression table, LambdaExpression columns)
            : base(queryBuilderBase, table, columns)
        { }
    }
}
