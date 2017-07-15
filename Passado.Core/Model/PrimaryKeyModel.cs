using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Passado.Core.Model
{
    public class PrimaryKeyModel
    {
        private readonly string _name;
        private readonly ImmutableList<SortedColumnModel> _columns;
        private readonly bool _isClustered;

        public PrimaryKeyModel(string name, IEnumerable<SortedColumnModel> columns, bool isClustered)
        {
            _name = name;
            _columns = columns.ToImmutableList();
            _isClustered = isClustered;
        }

        public string Name => _name;
        public ImmutableList<SortedColumnModel> Columns => _columns;
        public bool IsClustered => _isClustered;
    }
}
