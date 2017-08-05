using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Passado.Model.Table
{
    public class IndexModel
    {
        public IndexModel(string name, 
                          ImmutableArray<SortedColumnModel> keyColumns, 
                          bool isUnique, 
                          bool isClustered, 
                          ImmutableArray<ColumnModel> includedColumns)
        {
            Name = name;
            KeyColumns = keyColumns;
            IsUnique = isUnique;
            IsClustered = isClustered;
            IncludedColumns = includedColumns;
        }

        public string Name { get; }
        public ImmutableArray<SortedColumnModel> KeyColumns { get; }
        public bool IsUnique { get; }
        public bool IsClustered { get; }
        public ImmutableArray<ColumnModel> IncludedColumns { get; }
    }
}
