using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core.Model.Table
{
    public class Column<TDatabase, TTable> : IColumnBuilder<TDatabase, TTable>,
                                               IPrimaryKeyBuilder<TDatabase, TTable>,
                                               IIndexBuilder<TDatabase, TTable>,
                                               IForeignKeyBuilder<TDatabase, TTable>,
                                               ITableModelBuilder<TDatabase, TTable>
    {

    }
}
