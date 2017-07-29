using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

using Passado.Query.Select;

namespace Passado.Query.Dynamic
{
    public static class DynamicExtensions
    {
        public static Expression<Func<TParameter, TResult>> MakeLambda<TParameter, TResult>(Func<ParameterExpression, Expression> bodyBuilder)
        {
            var parameter = Expression.Parameter(typeof(TParameter), "t");
            var lambda = Expression.Lambda(bodyBuilder(parameter), parameter);
            return lambda as Expression<Func<TParameter, TResult>>;
        }

        public static IJoinQuery<TDatabase, TTable1, TTable2> Join<TDatabase, TTable1, TTable2>(this IFromQuery<TDatabase, TTable1> fromQuery, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Func<ParameterExpression, Expression> dynamicCondition)
        {
            return fromQuery.Join(selector, MakeLambda<IJoinedRow<TTable1, TTable2>, bool>(dynamicCondition));
        }

        public static IJoinQuery<TDatabase, TTable1, TTable2> LeftJoin<TDatabase, TTable1, TTable2>(this IFromQuery<TDatabase, TTable1> fromQuery, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Func<ParameterExpression, Expression> dynamicCondition)
        {
            return fromQuery.LeftJoin(selector, MakeLambda<IJoinedRow<TTable1, TTable2>, bool>(dynamicCondition));
        }

        public static IJoinQuery<TDatabase, TTable1, TTable2> RightJoin<TDatabase, TTable1, TTable2>(this IFromQuery<TDatabase, TTable1> fromQuery, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Func<ParameterExpression, Expression> dynamicCondition)
        {
            return fromQuery.RightJoin(selector, MakeLambda<IJoinedRow<TTable1, TTable2>, bool>(dynamicCondition));
        }

        public static IJoinQuery<TDatabase, TTable1, TTable2> OuterJoin<TDatabase, TTable1, TTable2>(this IFromQuery<TDatabase, TTable1> fromQuery, Expression<Func<TDatabase, IEnumerable<TTable2>>> selector, Func<ParameterExpression, Expression> dynamicCondition)
        {
            return fromQuery.OuterJoin(selector, MakeLambda<IJoinedRow<TTable1, TTable2>, bool>(dynamicCondition));
        }

        public static IGroupByQuery<TJoinedRow, TKey1> GroupBy<TJoinedRow, TKey1>(this IGroupable<TJoinedRow> groupable, Func<ParameterExpression, Expression> key1Selector)
            where TKey1 : IEquatable<TKey1>
        {
            return groupable.GroupBy(MakeLambda<TJoinedRow, TKey1>(key1Selector));
        }

        public static IGroupByQuery<TJoinedRow, TKey1, TKey2> GroupBy<TJoinedRow, TKey1, TKey2>(this IGroupable<TJoinedRow> groupable,
                                                                                                Func<ParameterExpression, Expression> key1Selector, 
                                                                                                Func<ParameterExpression, Expression> key2Selector)
            where TKey1 : IEquatable<TKey1>
            where TKey2 : IEquatable<TKey2>
        {
            return groupable.GroupBy(MakeLambda<TJoinedRow, TKey1>(key1Selector), MakeLambda<TJoinedRow, TKey2>(key2Selector));
        }

        public static IWhereQuery<TJoinedRow> Where<TJoinedRow>(this IWhereable<TJoinedRow> whereable, Func<ParameterExpression, Expression> dynamicCondition)
        {
            return whereable.Where(MakeLambda<TJoinedRow, bool>(dynamicCondition));
        }
    }
}
