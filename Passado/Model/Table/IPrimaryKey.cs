using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Model.Table
{
    public interface IPrimaryKey<TDatabase, TTable> : IIndexBuilder<TDatabase, TTable>,
                                                      IForeignKeyBuilder<TDatabase, TTable>,
                                                      ITableModelBuilder<TDatabase, TTable>
    {

    }
}
