using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Passado.Analyzers.Query
{
    public abstract class QueryAnalyzerBase
    {
        public abstract DiagnosticDescriptor Rule { get; }

        public abstract void AnalyzeQuery(SyntaxNodeAnalysisContext context, ExpressionSyntax query);
    }
}
