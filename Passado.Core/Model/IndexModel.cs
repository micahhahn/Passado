using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Passado.Core.Model
{
    public class IndexModel
    {
        private readonly string _name;
        private readonly ImmutableList<SortedColumnModel> _keyColumns;
        private readonly bool _isUnique;
        private readonly bool _isClustered;
        private readonly ImmutableList<ColumnModel> _includedColumns;

        public IndexModel(string name, IEnumerable<SortedColumnModel> keyColumns, bool isUnique, bool isClustered, IEnumerable<ColumnModel> includedColumns)
        {
            _name = name;
            _keyColumns = keyColumns.ToImmutableList();
            _isUnique = isUnique;
            _isClustered = isClustered;
            _includedColumns = includedColumns?.ToImmutableList();
        }

        public string Name => _name;
        public ImmutableList<SortedColumnModel> KeyColumns => _keyColumns;
        public bool IsUnique => _isUnique;
        public bool IsClustered => _isClustered;
        public ImmutableList<ColumnModel> IncludedColumns => _includedColumns;
    }
}
