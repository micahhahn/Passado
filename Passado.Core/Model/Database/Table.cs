using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core.Model.Database
{
    public class Table<TDatabase> : ITableBuilder<TDatabase>,
                                    IDatabaseModelBuilder<TDatabase>
    {

    }
}
