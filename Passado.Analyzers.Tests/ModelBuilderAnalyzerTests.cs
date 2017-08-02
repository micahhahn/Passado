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

using Passado.Tests.Model;

namespace Passado.Analyzers.Tests
{
    public class ModelBuilderAnalyzerTests : ModelBuilderTestsBase
    {
        public override async Task<List<(string ErrorId, string ErrorText, Location Location, Location AdditionalLocation)>> GetErrorsFromCompilation(Compilation compilation)
        {
            var compilationDiagnostics = compilation.GetDiagnostics();

            var analyzer = new ModelAnalyzer();

            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

            var diagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync();

            return diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
                              .Select(d => (d.Id, d.GetMessage(), d.Location, d.AdditionalLocations.FirstOrDefault()))
                              .ToList();
        }
    }
}
