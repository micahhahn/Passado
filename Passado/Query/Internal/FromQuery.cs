using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

using Passado.Model;

namespace Passado.Query.Internal
{
    public abstract class FromQueryBase : QueryBase
    {
        public FromQueryBase(QueryBuilderBase queryBuilderBase, LambdaExpression table)
        {
            InnerQuery = null;
            QueryBuilderBase = queryBuilderBase;
            Name = "T1";
            
            var property = ExpressionHelpers.ParseSelector(table);
            Model = queryBuilderBase.DatabaseModel.Tables.First(t => t.Property.Name == property.Name);
        }

        public QueryBuilderBase QueryBuilderBase { get; }
        public string Name { get; }
        public TableModel Model { get; }
    }

    public class FromQuery<TDatabase, TTable1>
            : FromQueryBase
            , Select.IFromQuery<TDatabase, TTable1>
            , Update.IUpdateQuery<TDatabase, TTable1>
            , Delete.IDeleteQuery<TDatabase, TTable1>
    {
        public FromQuery(QueryBuilderBase queryBuilderBase, LambdaExpression table)
            : base(queryBuilderBase, table)
        { }
    }
}
