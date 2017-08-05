using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Passado.Model
{
    public class DatabaseModel
    {
        public DatabaseModel(string name, ImmutableArray<TableModel> tables)
        {
            Name = name;
            Tables = tables.ToImmutableArray();
        }

        public string Name { get; }
        public ImmutableArray<TableModel> Tables { get; }
    }
}
