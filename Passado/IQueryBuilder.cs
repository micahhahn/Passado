using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

using Passado.Model;
using Passado.Query;
using Passado.Query.Select;

namespace Passado
{
    /// <summary>
    /// Provides an interface for constructing SQL queries against a data store.
    /// </summary>
    /// <typeparam name="TDatabase"></typeparam>
    public interface IQueryBuilder<TDatabase> : ISelectable<IJoinedRow>
    {
        DatabaseModel DatabaseModel { get; }
    }
}
