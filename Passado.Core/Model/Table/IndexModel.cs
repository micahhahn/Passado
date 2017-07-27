using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Passado.Core.Model.Table
{
    public class IndexModel
    {
        public IndexModel(string name, 
                          ImmutableList<SortedColumnModel> keyColumns, 
                          bool isUnique, 
                          bool isClustered, 
                          ImmutableList<ColumnModel> includedColumns)
        {
            Name = name;
            KeyColumns = keyColumns;
            IsUnique = isUnique;
            IsClustered = isClustered;
            IncludedColumns = includedColumns;
        }

        public string Name { get; }
        public ImmutableList<SortedColumnModel> KeyColumns { get; }
        public bool IsUnique { get; }
        public bool IsClustered { get; }
        public ImmutableList<ColumnModel> IncludedColumns { get; }
    }
}
