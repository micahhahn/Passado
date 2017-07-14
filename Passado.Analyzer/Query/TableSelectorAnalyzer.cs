using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Passado.Analyzers.Query
{
    public class TableSelectorAnalyzer : QueryMethodAnalyzerBase
    {
        public override DiagnosticDescriptor Rule => 
            new DiagnosticDescriptor(id: "PassadoTableSelectorAnalyzer",
                                     title: "Table selector must be simple.",
                                     messageFormat: "asdf",
                                     category: "Query",
                                     defaultSeverity: DiagnosticSeverity.Error,
                                     isEnabledByDefault: true,
                                     description: "asdf");

        public override IEnumerable<string> MethodHooks => new List<string>()
        {
            "From",
            "Join",
            "LeftJoin",
            "RightJoin",
            "OuterJoin"
        };

        public override void AnalyzeQueryMethod(SyntaxNodeAnalysisContext context, string methodName, ArgumentListSyntax arguments)
        {
            var selector = arguments.Arguments[0].Expression as SimpleLambdaExpressionSyntax;
            
            if (selector == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, arguments.Arguments[0].GetLocation(), "xxx"));
            }
        }
    }
}
