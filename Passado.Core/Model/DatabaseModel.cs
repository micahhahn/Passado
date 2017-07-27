using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Passado.Core.Model
{
    public class DatabaseModel
    {
        public DatabaseModel(string name, ImmutableList<TableModel> tables)
        {
            Name = name;
            Tables = tables.ToImmutableList();
        }

        public string Name { get; }
        public ImmutableList<TableModel> Tables { get; }
    }
}
