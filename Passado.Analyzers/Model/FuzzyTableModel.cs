using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

namespace Passado.Analyzers.Model
{
    public class FuzzyTableModel
    {
        public Optional<string> Name { get; set; }
        public Optional<string> Schema { get; set; }
        public Optional<FuzzyProperty> Property { get; set; }
        public List<FuzzyColumnModel> Columns { get; set; }
        public FuzzyPrimaryKeyModel PrimaryKey { get; set; }
    }
}