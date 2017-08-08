using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;

using Passado.Model;

namespace Passado.Analyzers.Model
{
    public class FuzzyPrimaryKeyModel
    {
        public Optional<string> Name { get; set; }
        public Optional<ImmutableArray<(SortOrder, FuzzyColumnModel)>> Columns { get; set; }
        public Optional<bool> IsClustered { get; set; }
    }
}
