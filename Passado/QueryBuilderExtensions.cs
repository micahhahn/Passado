using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Text;

using Passado.Model;
using Passado.Query;
using Passado.Error;

namespace Passado
{
    public static class QueryBuilderExtensions
    {
        public static (DatabaseModel DatabaseModel, ImmutableArray<JoinedTable> JoinedTables) ParseImplicitJoin(JoinedQueryBuilder joinedQueryBuilder, JoinType joinType, LambdaExpression selector)
        {
            throw new NotImplementedException();
        }

        public static (DatabaseModel DatabaseModel, ImmutableArray<JoinedTable> JoinedTables) ParseExplicitJoin(JoinedQueryBuilder joinedQueryBuilder, JoinType joinType, LambdaExpression selector, LambdaExpression condition)
        {
            if (selector == null)
                throw BuilderError.ArgumentNull(nameof(selector)).AsException();

            var tableProperty = ExpressionHelpers.ParseSelector(selector);

            var tableModel = joinedQueryBuilder.DatabaseModel.Tables.FirstOrDefault(t => t.Property.Name == tableProperty.Name);

            if (tableModel == null)
                throw BuilderError.SelectorNotMappedToTable(tableProperty.Name, joinedQueryBuilder.DatabaseModel.Name).AsException();

            var joinedTables = joinedQueryBuilder.JoinedTables.Add(new JoinedTable()
            {
                Model = tableModel,
                JoinType = joinType
            });

            return (joinedQueryBuilder.DatabaseModel, joinedTables);
        }

        #region Select

        public static Query.Select.IFromQuery<TDatabase, TTable1> From<TDatabase, TTable1>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TTable1>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1>(ParseExplicitJoin(new JoinedQueryBuilder<TDatabase>((qb.DatabaseModel, ImmutableArray.Create<JoinedTable>())), JoinType.Inner, selector, null));
        }
        
        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Inner, selector));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Inner, selector, condition));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Left, selector));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Left, selector, condition));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> RightJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Right, selector));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> RightJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Right, selector, condition));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> OuterJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Outer, selector));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> OuterJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Outer, selector, condition));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2> CrossJoin<TDatabase, TTable1, TTable2>(this Query.Select.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Cross, selector));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Inner, selector));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Inner, selector, condition));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Left, selector));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Left, selector, condition));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> RightJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Right, selector));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> RightJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Right, selector, condition));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> OuterJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Outer, selector));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> OuterJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Outer, selector, condition));
        }

        public static Query.Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> CrossJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Select.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Cross, selector));
        }

        public static Query.Select.IAsQuery<TNamedRow> As<TJoinedRow, TNamedRow>(this Query.Select.IAsable<TJoinedRow> asable, Expression<Func<TJoinedRow, TNamedRow>> name)
        {
            var _ = asable as JoinedQueryBuilder;

            var __ = ExpressionHelpers.ParseMultiPropertySelector(name);

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

        public static Query.Insert.IInsertQuery<TDatabase, TTable1> Insert<TDatabase, TTable1>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TTable1>>> selector, Expression<Func<TTable1, object>> columns)
        {
            throw new NotImplementedException();
        }

        #region Update

        public static Query.Update.IUpdateQuery<TDatabase, TTable1> Update<TDatabase, TTable1>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TTable1>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1>(ParseExplicitJoin(new JoinedQueryBuilder<TDatabase>((qb.DatabaseModel, ImmutableArray.Create<JoinedTable>())), JoinType.Inner, selector, null));
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Update.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Inner, selector));
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Update.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Inner, selector, condition));
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Update.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Left, selector));
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Update.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Left, selector, condition));
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Update.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Inner, selector));
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Update.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable3>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Inner, selector, condition));
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Update.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Left, selector));
        }

        public static Query.Update.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Update.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable3>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Left, selector, condition));
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
            return new JoinedQueryBuilder<TDatabase, TTable1>(ParseExplicitJoin(new JoinedQueryBuilder<TDatabase>((qb.DatabaseModel, ImmutableArray.Create<JoinedTable>())), JoinType.Inner, selector, null));
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Delete.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Inner, selector));
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this Query.Delete.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Inner, selector, condition));
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Delete.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Left, selector));
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this Query.Delete.IJoinable<TDatabase, TTable1> joinable, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Left, selector, condition));
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Delete.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Inner, selector));
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> Join<TDatabase, TTable1, TTable2, TTable3>(this Query.Delete.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Inner, selector, condition));
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Delete.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseImplicitJoin(joinable as JoinedQueryBuilder, JoinType.Left, selector));
        }

        public static Query.Delete.IJoinQuery<TDatabase, TTable1, TTable2, TTable3> LeftJoin<TDatabase, TTable1, TTable2, TTable3>(this Query.Delete.IJoinable<TDatabase, TTable1, TTable2> joinable, Expression<Func<TDatabase, IEnumerable<TTable3>>> selector, Expression<Func<IJoinedRow<TTable1, TTable2, TTable3>, bool>> condition)
        {
            return new JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>(ParseExplicitJoin(joinable as JoinedQueryBuilder, JoinType.Left, selector, condition));
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
