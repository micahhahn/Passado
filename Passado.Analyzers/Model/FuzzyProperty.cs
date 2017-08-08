using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

namespace Passado.Analyzers.Model
{
    public class FuzzyProperty
    {
        public FuzzyProperty(IPropertySymbol property)
        {
            Name = property.Name;
            Type = property.Type;
        }

        public string Name { get; }
        public ITypeSymbol Type { get; }
    }
}
