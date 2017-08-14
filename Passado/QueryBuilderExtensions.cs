using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

using Passado.Query.Select;
using Passado.Query.Insert;
using Passado.Query.Update;
using Passado.Query.Delete;

namespace Passado
{
    public static class QueryBuilderExtensions
    {
        public static IFromQuery<TDatabase, TTable1> From<TDatabase, TTable1>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TTable1>>> selector)
        {
            throw new NotImplementedException();
        }

        public static IInsertQuery<TDatabase, TTable1> Insert<TDatabase, TTable1>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TTable1>>> selector, Expression<Func<TTable1, object>> columns)
        {
            throw new NotImplementedException();
        }

        public static IUpdateQuery<TDatabase, TTable1> Update<TDatabase, TTable1>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TTable1>>> selector)
        {
            throw new NotImplementedException();
        }

        public static IDeleteQuery<TDatabase, TTable1> Delete<TDatabase, TTable1>(this IQueryBuilder<TDatabase> qb, Expression<Func<TDatabase, IEnumerable<TTable1>>> selector)
        {
            throw new NotImplementedException();
        }
    }
}
