﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Passado
{
    /// <summary>
    /// Represents a query which returns no tables.
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// Executes the query returning the number of rows affected.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        int Execute();

        Task<int> ExecuteAsync();
    }

    /// <summary>
    /// Represents a query which returns a table.
    /// </summary>
    /// <typeparam name="TTable"></typeparam>
    public interface IQuery<TTable>
    {
        IEnumerable<TTable> Execute();
        Task<IEnumerable<TTable>> ExecuteAsync();
    }
}
