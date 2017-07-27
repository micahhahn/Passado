﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core.Model.Table
{
    public class Index<TDatabase, TTable> : IIndexBuilder<TDatabase, TTable>,
                                            IForeignKeyBuilder<TDatabase, TTable>,
                                            ITableModelBuilder<TDatabase, TTable>
    {

    }
}
