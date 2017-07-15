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

        static bool IsSimpleSelector(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
        {
            var lambdaExpression = expression as SimpleLambdaExpressionSyntax;

            if (lambdaExpression == null)
                return false;

            var selector = lambdaExpression.Body as MemberAccessExpressionSyntax;

            if (selector == null)
                return false;

            return context.SemanticModel.GetSymbolInfo(selector.Expression).Symbol is IParameterSymbol;
        }

        public override void AnalyzeQueryMethod(SyntaxNodeAnalysisContext context, string methodName, ArgumentListSyntax arguments)
        {
            if (!IsSimpleSelector(context, arguments.Arguments[0].Expression))
                context.ReportDiagnostic(Diagnostic.Create(Rule, arguments.Arguments[0].GetLocation(), "Must be a simple selector"));
        }
    }
}
