using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Passado.Core.Model
{
    public class DatabaseModel
    {
        private readonly string _name;
        private ImmutableList<TableModel> _tables;

        public DatabaseModel(string name, IEnumerable<TableModel> tables)
        {
            _name = name;
            _tables = tables.ToImmutableList();
        }

        public string Name => _name;
        public ImmutableList<TableModel> Tables => _tables;
    }
}
