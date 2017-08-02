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
        public static Diagnostic MakeDiagnostic(this ModelBuilderError error, Location location, IEnumerable<Location> additionalLocations = null)
        {
            return Diagnostic.Create(error.AsDiagnostic(), location, additionalLocations);
        }

        public static DiagnosticDescriptor AsDiagnostic(this ModelBuilderError error)
        {
            return new DiagnosticDescriptor(error.ErrorId, error.Title, error.Message, "Passado", DiagnosticSeverity.Error, true);
        }

        public static ImmutableArray<DiagnosticDescriptor> AllDiagnostics()
        {
            return typeof(ModelBuilderError).GetRuntimeMethods()
                                            .Where(m => m.ReturnType == typeof(ModelBuilderError))
                                            .Select(m => (m.Invoke(null, m.GetParameters().Select(p => null as object).ToArray()) as ModelBuilderError).AsDiagnostic())
                                            .ToImmutableArray();
        }
    }
}
