﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Model.Database
{
    public interface ITable<TDatabase> : ITableBuilder<TDatabase>,
                                         IDatabaseModelBuilder<TDatabase>
    {

    }
}
