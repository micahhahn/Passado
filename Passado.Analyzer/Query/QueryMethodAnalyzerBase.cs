using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Passado.Analyzers.Query
{
    public abstract class QueryMethodAnalyzerBase
    {
        public abstract DiagnosticDescriptor Rule { get; }

        public abstract IEnumerable<string> MethodHooks { get; }

        public abstract void AnalyzeQueryMethod(SyntaxNodeAnalysisContext context, string methodName, ArgumentListSyntax arguments);
    }
}
