using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Passado.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class QueryAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PassadoQueryAnalyzer";

        private static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(id: "PassadoQueryAnalyzer", 
                                     title: "Query contains an error.", 
                                     messageFormat: "{0}",
                                     category: "Query",
                                     defaultSeverity: DiagnosticSeverity.Error,
                                     isEnabledByDefault: true,
                                     description: "Ensure constaits that are impossible to encode in the type system.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {

        }
    }
}
