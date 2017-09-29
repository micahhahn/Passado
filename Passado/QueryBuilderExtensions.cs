using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Text;

using Passado.Model;
using Passado.Query;
using Passado.Query.Internal;
using Passado.Error;

namespace Passado
{
    public static class QueryBuilderExtensions
    {
        #region Select
        
        public static Query.Select.ISelectQuery<TResult> Select<TDatabase, TResult>(this IQueryBuilder<TDatabase> qb, Expression<Func<IJoinedRow, TResult>> selector)
        {
            return new ScalarSelectQuery<TResult>(qb as QueryBuilderBase, selector);
        }

        public static Query.Select.IFromQuery<TDatabase, TTable1> From<TDatabase, TTable1>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TTable1>>> selector)
        {
            return new FromQuery<TDatabase, TTable1>(qb as QueryBuilderBase, selector);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Inner, selector);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Inner, selector, condition);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Left, selector);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Left, selector, condition);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> RightJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Right, selector);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> RightJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Right, selector, condition);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> OuterJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Outer, selector);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> OuterJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Outer, selector, condition);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> CrossJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Cross, selector);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Inner, selector);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Inner, selector, condition);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Left, selector);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Left, selector, condition);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> RightJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Right, selector);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> RightJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Right, selector, condition);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> OuterJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Outer, selector);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> OuterJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Outer, selector, condition);
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> CrossJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Cross, selector);
        }

        public static Query.Select.IAsQuery<TNamedRow> As<TJoinedRow, TNamedRow>(this Query.Select.IAsable<TJoinedRow> asable, Expression<Func<TJoinedRow, TNamedRow>> name)
        {
            return new AsQuery<TNamedRow, object>(asable as QueryBase, name);
        }

        public static Query.Select.IWhereQuery<TJoinedRow> Where<TJoinedRow>(this Query.Select.IWhereable<TJoinedRow> whereable, Expression<Func<TJoinedRow, bool>> condition)
        {
            return new WhereQuery<TJoinedRow, object>(whereable as QueryBase, condition);
        }

        public static Query.Select.IGroupByQuery<IGroupedRow<TJoinedRow, TKeys>> GroupBy<TJoinedRow, TKeys>(this Query.Select.IGroupable<TJoinedRow> groupable, Expression<Func<TJoinedRow, TKeys>> keys)
        {
            return new GroupByQuery<IGroupedRow<TJoinedRow, TKeys>>(groupable as QueryBase, keys);
        }

        public static Query.Select.IHavingQuery<TGroupedRow> Having<TGroupedRow>(this Query.Select.IHavingable<TGroupedRow> havingable, Expression<Func<TGroupedRow, bool>> condition)
        {
            return new HavingQuery<TGroupedRow>(havingable as QueryBase, condition);
        }
        
        public static Query.Select.ISelectQuery<TResult> Select<TJoinedRow, TResult>(this Query.Select.ISelectable<TJoinedRow> selectable, Expression<Func<TJoinedRow, TResult>> selector)
        {
            return new SelectQuery<TResult>(selectable as QueryBase, selector);
        }

        public static Query.Select.IOrderByQuery<TResult> OrderBy<TResult>(this Query.Select.IOrderable<TResult> orderable, Expression<Func<IOrderFilter<TResult>, object>> selector)
        {
            return new OrderByQuery<TResult>(orderable as QueryBase, selector);
        }

        public static Query.Select.IOffsetQuery<TResult> Offset<TResult>(this Query.Select.IOffsetable<TResult> offsetable, long offset)
        {
            return new OffsetQuery<TResult>(offsetable as QueryBase, offset);
        }

        public static Query.Select.ILimitQuery<TResult> Limit<TResult>(this Query.Select.ILimitable<TResult> limitable, long limit)
        {
            return new LimitQuery<TResult>(limitable as QueryBase, limit);
        }

        #endregion

        public static Query.Insert.IInsertQuery<TDatabase, TIntoTable> Insert<TDatabase, TIntoTable>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TIntoTable>>> table, Expression<Func<TIntoTable, object>> columns)
        {
            return new InsertQuery<TDatabase, TIntoTable>(qb as QueryBuilderBase, table, columns);
        }

        public static Query.Insert.IValueQuery<TIntoTable> Value<TIntoTable>(this Query.Insert.IValuable<TIntoTable> valuable, Expression<Func<TIntoTable>> value)
        {
            return new ValueQuery<TIntoTable>(valuable as QueryBase, value);
        }

        #region Update

        public static Query.Update.IUpdateQuery<TDatabase, TTable1> Update<TDatabase, TTable1>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TTable1>>> selector)
        {
            return new FromQuery<TDatabase, TTable1>(qb as QueryBuilderBase, selector);
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Update.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Inner, selector);
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Update.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Inner, selector, condition);
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Update.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Left, selector);
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Update.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Left, selector, condition);
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Update.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Inner, selector);
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Update.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable3>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Inner, selector, condition);
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Update.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Left, selector);
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Update.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable3>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Left, selector, condition);
        }

        public static Query.Update.IAsQuery<TNamedRow, TUpdateTable> As<TJoinedRow, TUpdateTable, TNamedRow>(this Query.Update.IAsable<TJoinedRow, TNamedRow> asable, Expression<Func<TJoinedRow, TNamedRow>> name)
        {
            return new AsQuery<TNamedRow, TUpdateTable>(asable as QueryBase, name);
        }

        public static Query.Update.IWhereQuery<TJoinedRow, TTable1> Where<TJoinedRow, TTable1>(this Query.Update.IWhereable<TJoinedRow, TTable1> whereable, Expression<Func<TJoinedRow, bool>> condition)
        {
            return new WhereQuery<TJoinedRow, TTable1>(whereable as QueryBase, condition);
        }

        public static Query.Update.ISetable<TJoinedRow, TTable1> Set<TJoinedRow, TTable1, TProperty>(this Query.Update.ISetable<TJoinedRow, TTable1> setable, Expression<Func<TTable1, TProperty>> column, Expression<Func<TJoinedRow, TProperty>> value)
        {
            return new SetQuery<TJoinedRow, TTable1>(setable as QueryBase, column, value);
        }

        #endregion Update

        #region Delete

        public static Query.Delete.IDeleteQuery<TDatabase, TTable1> Delete<TDatabase, TTable1>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TTable1>>> selector)
        {
            return new FromQuery<TDatabase, TTable1>(qb as QueryBuilderBase, selector);
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Delete.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Inner, selector);
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Delete.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Inner, selector, condition);
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Delete.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Left, selector);
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Delete.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2>(joinable as QueryBase, JoinType.Left, selector, condition);
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Delete.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Inner, selector);
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Delete.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Inner, selector, condition);
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Delete.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Left, selector);
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Delete.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            return new JoinQuery<TDatabase, TTable1, TTable2, TTable3>(joinable as QueryBase, JoinType.Left, selector, condition);
        }

        public static Query.Delete.IAsQuery<TNamedRow> As<TJoinedRow, TNamedRow>(this Query.Delete.IAsable<TJoinedRow> asable, Expression<Func<TJoinedRow, TNamedRow>> name)
        {
            return new AsQuery<TNamedRow, object>(asable as QueryBase, name);
        }

        public static Query.Delete.IWhereQuery<TJoinedRow> Where<TJoinedRow>(this Query.Delete.IWhereable<TJoinedRow> whereable, Expression<Func<TJoinedRow, bool>> condition)
        {
            return new WhereQuery<TJoinedRow, object>(whereable as QueryBase, condition);
        }

        #endregion

        static QueryBuilderBase GetQueryBuilderBase(QueryBase query)
        {
            if (query is FromQueryBase fromQuery)
            {
                return fromQuery.QueryBuilderBase;
            }
            else if (query is InsertQueryBase insertQuery)
            {
                return insertQuery.QueryBuilderBase;
            }
            else if (query is ScalarSelectQueryBase scalarSelectQuery)
            {
                return scalarSelectQuery.QueryBuilderBase;
            }
            else
            {
                return GetQueryBuilderBase(query.InnerQuery);
            }
        }

        public static IQuery Build(this ITerminalQuery terminalQuery)
        {
            return GetQueryBuilderBase(terminalQuery as QueryBase).Build(terminalQuery as QueryBase);
        }

        public static int Execute(this ITerminalQuery terminalQuery)
        {
            return Build(terminalQuery).Execute();
        }

        public static Task<int> ExecuteAsync(this ITerminalQuery terminalQuery)
        {
            return Build(terminalQuery).ExecuteAsync();
        }

        public static IQuery<TTable> Build<TTable>(this ITerminalQuery<TTable> terminalQuery)
        {
            return GetQueryBuilderBase(terminalQuery as QueryBase).Build<TTable>(terminalQuery as QueryBase);
        }

        public static IEnumerable<TTable> Execute<TTable>(this ITerminalQuery<TTable> terminalQuery)
        {
            return Build(terminalQuery).Execute();
        }

        public static Task<IEnumerable<TTable>> ExecuteAsync<TTable>(this ITerminalQuery<TTable> terminalQuery)
        {
            return Build(terminalQuery).ExecuteAsync();
        }
    }
}
