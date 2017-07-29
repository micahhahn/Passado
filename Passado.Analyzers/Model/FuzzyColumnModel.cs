using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

using Passado.Model;

namespace Passado.Analyzers.Model
{
    public class FuzzyColumnModel
    {
        public Optional<string> Name { get; set; }
        public Optional<FuzzyProperty> Property { get; set; }
        public Optional<SqlType> Type { get; set; }
        public Optional<int?> MaxLength { get; set;  }
        public Optional<bool> IsNullable { get; set; }
        public Optional<bool> HasDefaultValue { get; set; }
        public Optional<bool> IsIdentity { get; set; }
    }
}
