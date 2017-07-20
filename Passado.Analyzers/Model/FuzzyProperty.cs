using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

namespace Passado.Analyzers.Model
{
    public class FuzzyProperty
    {
        public string Name { get; set; }
        public ITypeSymbol Type { get; set; }
    }
}
