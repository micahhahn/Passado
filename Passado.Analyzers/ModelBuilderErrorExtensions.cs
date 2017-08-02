using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Reflection;

namespace Passado.Analyzers
{
    public static class ModelBuilderErrorExtensions
    {
        public static Diagnostic MakeDiagnostic(this ModelBuilderError error, Location location, string message, IEnumerable<Location> additionalLocations = null)
        {
            return Diagnostic.Create(error.AsDiagnostic(), location, additionalLocations, message);
        }

        public static DiagnosticDescriptor AsDiagnostic(this ModelBuilderError error)
        {
            return new DiagnosticDescriptor(error.ErrorId, error.Title, error.MessageFormat, "Passado", DiagnosticSeverity.Error, true);
        }

        public static ImmutableArray<DiagnosticDescriptor> AllDiagnostics()
        {
            return typeof(ModelBuilderError).GetRuntimeFields()
                                            .Where(f => f.FieldType == typeof(ModelBuilderError))
                                            .Select(f => (f.GetValue(null) as ModelBuilderError).AsDiagnostic())
                                            .ToImmutableArray();
        }
    }
}
