using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Model.Table
{
    public interface ITable<TDatabase, TTable> : IColumnBuilder<TDatabase, TTable>
    {

    }
}
