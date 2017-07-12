using System;
using System.Linq.Expressions;

using Passado.Core.Query.Select;
using Passado.Core.Query.Delete;
using Passado.Core.Query.Insert;
using Passado.Core.Query.Update;

namespace Passado.Core
{
    /// <summary>
    /// Provides an interface for constructing SQL queries against a data store.
    /// </summary>
    /// <typeparam name="TDatabase"></typeparam>
    public interface IQueryBuilder<TDatabase>
    {
        IQuery<TTable> Select<TTable>(Expression<Func<ISelectQueryBuilder<TDatabase>, ISelectQuery<TTable>>> query);

        IQuery Insert(Expression<Func<IInsertQueryBuilder<TDatabase>, IInsertQuery>> query);

        IQuery Update(Expression<Func<IUpdateQueryBuilder<TDatabase>, IUpdateQuery>> query);

        IQuery Delete(Expression<Func<IDeleteQueryBuilder<TDatabase>, IDeleteQuery>> query);
    }
}
