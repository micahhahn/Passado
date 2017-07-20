using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

namespace Passado.Analyzers.Model
{
    public class FuzzyDatabaseModel
    {
        public Optional<string> Name { get; set; }
        public List<FuzzyTableModel> Tables { get; set; }
    }
}
