using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

using Passado.Core.Query.Select;

namespace Passado.Core.Query.Dynamic
{
    public static class DynamicExtensions
    {
        public static IWhereQuery<TJoinedRow> Where<TJoinedRow>(this IWhereable<TJoinedRow> whereable, Func<ParameterExpression, Expression> dynamicCondition)
        {
            var parameter = Expression.Parameter(typeof(TJoinedRow), "t");
            var lambda = Expression.Lambda(dynamicCondition(parameter), parameter);

            return whereable.Where((Expression<Func<TJoinedRow, bool>>)lambda);
        }
    }
}
