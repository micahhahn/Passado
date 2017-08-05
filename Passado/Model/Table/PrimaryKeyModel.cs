using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Passado.Model.Table
{
    public class PrimaryKeyModel
    {
        public PrimaryKeyModel(string name,
                               ImmutableArray<SortedColumnModel> columns, 
                               bool isClustered)
        {
            Name = name;
            Columns = columns;
            IsClustered = isClustered;
        }

        public string Name { get; }
        public ImmutableArray<SortedColumnModel> Columns { get; }
        public bool IsClustered { get; }
    }
}
