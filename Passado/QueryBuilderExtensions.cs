using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

using Passado.Query;
using Passado.Query.Select;
using Passado.Query.Insert;
using Passado.Query.Update;
using Passado.Query.Delete;

using Passado.Error;

namespace Passado
{
    public static class QueryBuilderExtensions
    {
        #region Select

        public static IFromQuery<TDatabase, TTable1> From<TDatabase, TTable1>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TTable1>>> selector)
        {
            if (selector == null)
                throw BuilderError.ArgumentNull(nameof(selector)).AsException();

            var tableProperty = ExpressionHelpers.ParseSelector(selector);
            
            
            //return new SelectQueryBuilder<TDatabase, TTable1>()

            throw new NotImplementedException();
        }
        
        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> RightJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> RightJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> OuterJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> OuterJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> CrossJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> RightJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> RightJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> OuterJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> OuterJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> CrossJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IAsQuery<TNamedRow> As<TJoinedRow, TNamedRow>(this Query.Select.IAsable<TJoinedRow> asable, Expression<Func<TJoinedRow, TNamedRow>> name)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IWhereQuery<TJoinedRow> Where<TJoinedRow>(this Query.Select.IWhereable<TJoinedRow> whereable, Expression<Func<TJoinedRow, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IGroupByQuery<IGroupedRow<TJoinedRow, TKeys>> GroupBy<TJoinedRow, TKeys>(this Query.Select.IGroupable<TJoinedRow> groupable, Expression<Func<TJoinedRow, TKeys>> keys)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.IHavingQuery<TGroupedRow> Having<TGroupedRow>(this Query.Select.IHavingable<TGroupedRow> havingable, Expression<Func<TGroupedRow, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Select.ISelectQuery<TResult> Select<TJoinedRow, TResult>(this Query.Select.ISelectable<TJoinedRow> selectable, Expression<Func<TJoinedRow, TResult>> selector)
        {
            throw new NotImplementedException();
        }

        #endregion

        public static IInsertQuery<TDatabase, TTable1> Insert<TDatabase, TTable1>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TTable1>>> selector, Expression<Func<TTable1, object>> columns)
        {
            throw new NotImplementedException();
        }

        #region Update

        public static Query.Update.IUpdateQuery<TDatabase, TTable1> Update<TDatabase, TTable1>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TTable1>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Update.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Update.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Update.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Update.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Update.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Update.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable3>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Update.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Update.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable3>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Update.IAsQuery<TNamedRow, TUpdateTable> As<TJoinedRow, TUpdateTable, TNamedRow>(this Query.Update.IAsable<TJoinedRow, TNamedRow> asable, Expression<Func<TJoinedRow, TNamedRow>> name)
        {
            throw new NotImplementedException();
        }

        public static Query.Update.IWhereQuery<TJoinedRow, TTable1> Where<TJoinedRow, TTable1>(this Query.Update.IWhereable<TJoinedRow, TTable1> whereable, Expression<Func<TJoinedRow, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Update.ISetable<TJoinedRow, TTable1> Set<TJoinedRow, TTable1, TProperty>(this Query.Update.ISetable<TJoinedRow, TTable1> setable, Expression<Func<TTable1, TProperty>> property, Expression<Func<TJoinedRow, TProperty>> value)
        {
            throw new NotImplementedException();
        }

        #endregion Update

        #region Delete

        public static Query.Delete.IDeleteQuery<TDatabase, TTable1> Delete<TDatabase, TTable1>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TTable1>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Delete.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Delete.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Delete.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Delete.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Delete.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Delete.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Delete.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            throw new NotImplementedException();
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Delete.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            throw new NotImplementedException();
        }

        public static Query.Delete.IAsQuery<TNamedRow> As<TJoinedRow, TNamedRow>(this Query.Delete.IAsable<TJoinedRow> asable, Expression<Func<TJoinedRow, TNamedRow>> name)
        {
            throw new NotImplementedException();
        }

        public static Query.Delete.IWhereQuery<TJoinedRow> Where<TJoinedRow>(this Query.Delete.IWhereable<TJoinedRow> whereable, Expression<Func<TJoinedRow, bool>> condition)
        {
            throw new NotImplementedException();
        }

        #endregion

        public static IQuery Build(this ITerminalQuery terminalQuery)
        {
            throw new NotImplementedException();
        }

        public static int Execute(this ITerminalQuery terminalQuery)
        {
            throw new NotImplementedException();
        }

        public static Task<int> ExecuteAsync(this ITerminalQuery terminal)
        {
            throw new NotImplementedException();
        }

        public static IQuery<TTable> Build<TTable>(this ITerminalQuery<TTable> terminalQuery)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<TTable> Execute<TTable>(this ITerminalQuery<TTable> terminalQuery)
        {
            throw new NotImplementedException();
        }

        public static Task<IEnumerable<TTable>> ExecuteAsync<TTable>(this ITerminalQuery<TTable> terminal)
        {
            throw new NotImplementedException();
        }
    }
}
