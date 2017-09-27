using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

using Passado.Tests;
using Passado.Tests.ModelBuilder;

namespace Passado.Analyzers.Tests.Model
{
    public class IndexBuilderAnalyzerTests : IndexBuilderTests
    {
        protected override Task<CompilationError[]> GetCompilationErrors(Compilation compilation) => AnalyzerHelpers.GetErrorsFromCompilation(compilation);
    }
}
