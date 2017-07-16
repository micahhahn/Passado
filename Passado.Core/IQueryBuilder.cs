using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

using Passado.Core.Query.Select;
using Passado.Core.Query.Insert;
using Passado.Core.Query.Update;
using Passado.Core.Query.Delete;

namespace Passado.Core
{
    /// <summary>
    /// Provides an interface for constructing SQL queries against a data store.
    /// </summary>
    /// <typeparam name="TDatabase"></typeparam>
    public interface IQueryBuilder<TDatabase>
    {
        IFromQuery<TDatabase, TTable1> From<TTable1>(Expression<Func<TDatabase, IEnumerable<TTable1>>> selector);
        IInsertQuery<TDatabase, TTable1> Insert<TTable1>(Expression<Func<TDatabase, IEnumerable<TTable1>>> selector);
        IUpdateQuery<TDatabase, TTable1> Update<TTable1>(Expression<Func<TDatabase, IEnumerable<TTable1>>> selector);
        IDeleteQuery<TDatabase, TTable1> Delete<TTable1>(Expression<Func<TDatabase, IEnumerable<TTable1>>> selector);
    }
}
