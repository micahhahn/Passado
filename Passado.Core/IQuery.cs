using System.Collections.Generic;

namespace Passado.Core
{
    /// <summary>
    /// Represents a query which returns no tables.
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// Executes the query returing the number of rows affected.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        int Execute();
    }

    /// <summary>
    /// Represents a query which returns a table.
    /// </summary>
    /// <typeparam name="TTable"></typeparam>
    public interface IQuery<TTable>
    {
        IEnumerable<TTable> Execute();
    }
}
