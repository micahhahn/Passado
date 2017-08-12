using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Passado.Tests;
using Passado.Tests.Model;

namespace Passado.Analyzers.Tests.Model
{
    public static class AnalyzerHelpers
    {
        public static async Task<CompilationError[]> GetErrorsFromCompilation(Compilation compilation)
        {
            var compilationDiagnostics = compilation.GetDiagnostics();
            
            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new ModelAnalyzer()));

            var diagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync();
            
            return diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
                              .Select(d =>
                              {
                                  var locations = new List<Location>() { d.Location };
                                  locations.AddRange(d.AdditionalLocations);

                                  return new CompilationError()
                                  {
                                      ErrorId = d.Id,
                                      ErrorText = d.GetMessage(),
                                      Locations = locations.Select(l => l.SourceTree.ToString().Substring(l.SourceSpan.Start, l.SourceSpan.Length)).ToArray()
                                  };
                              })
                              .ToArray();
        }
    }
}
