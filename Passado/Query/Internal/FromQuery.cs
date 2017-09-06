using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

using Passado.Model;

namespace Passado.Query.Internal
{
    public abstract class FromQueryBase : QueryBase
    {
        public FromQueryBase(QueryBuilderBase queryBuilderBase, Expression table)
        {
            InnerQuery = null;
            QueryBuilderBase = queryBuilderBase;
            Name = "T1";
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
        public FromQuery(QueryBuilderBase queryBuilderBase, Expression table)
            : base(queryBuilderBase, table)
        { }
    }
}
