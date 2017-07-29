using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Model.Table
{
    public interface IColumn<TDatabase, TTable> : IColumnBuilder<TDatabase, TTable>,
                                                  IPrimaryKeyBuilder<TDatabase, TTable>,
                                                  IIndexBuilder<TDatabase, TTable>,
                                                  IForeignKeyBuilder<TDatabase, TTable>,
                                                  ITableModelBuilder<TDatabase, TTable>
    {

    }
}
