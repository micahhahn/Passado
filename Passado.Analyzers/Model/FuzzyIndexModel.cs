﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;

using Passado.Model;

namespace Passado.Analyzers.Model
{
    public class FuzzyIndexModel
    {
        public Optional<string> Name { get; set; }
        public Optional<ImmutableArray<(SortOrder, FuzzyColumnModel)>> KeyColumns { get; set; }
        public Optional<bool> IsClustered { get; set; }   
        public Optional<bool> IsUnique { get; set; }
        public Optional<ImmutableArray<FuzzyColumnModel>> IncludedColumns { get; set; }
    }
}
