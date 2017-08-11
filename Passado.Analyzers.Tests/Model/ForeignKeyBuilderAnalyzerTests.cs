using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

using Passado.Tests.Model;

namespace Passado.Analyzers.Tests.Model
{
    public class ForeignKeyBuilderAnalyzerTests : ForeignKeyBuilderTests
    {
        public override Task<List<(string ErrorId, string ErrorText, Location Location, Location AdditionalLocation)>> GetErrorsFromCompilation(Compilation compilation) => AnalyzerHelpers.GetErrorsFromCompilation(compilation);
    }
}
