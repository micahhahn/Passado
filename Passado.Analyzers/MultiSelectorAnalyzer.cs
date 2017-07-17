using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Passado.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MultiSelectorAnalyzer : DiagnosticAnalyzer
    {
        public static string Id => "PassadoMultiSelectorAnalyzer";

        static DiagnosticDescriptor _rule = new DiagnosticDescriptor(id: Id,
                                                                     title: "This is a title.",
                                                                     messageFormat: "{0}",
                                                                     category: "Passado",
                                                                     defaultSeverity: DiagnosticSeverity.Error,
                                                                     isEnabledByDefault: true);

        static Dictionary<string, string> _methodHooks = new Dictionary<string, string>()
        {
            { "Insert", "Passado.Core.IQueryBuilder" }
        };
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public static bool IsInvalidSimpleSelector(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
        {
            var selector = expression as MemberAccessExpressionSyntax;

            if (selector == null)
                return true;

            return !(context.SemanticModel.GetSymbolInfo(selector.Expression).Symbol is IParameterSymbol);
        }

        public static void ValidateMultiSelector(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            var lambdaExpression = argument.Expression as SimpleLambdaExpressionSyntax;

            // If it's not a lambda expression, we can't easily do any compile time analysis
            if (lambdaExpression == null)
                return;

            if (lambdaExpression.Body is MemberAccessExpressionSyntax)
            {
                if (IsInvalidSimpleSelector(context, lambdaExpression.Body as ExpressionSyntax))
                {
                    context.ReportDiagnostic(Diagnostic.Create(_rule, lambdaExpression.Body.GetLocation(), "must be a simple member expression"));
                }
            }
            else if (lambdaExpression.Body is AnonymousObjectCreationExpressionSyntax)
            {
                var anonymousExpression = lambdaExpression.Body as AnonymousObjectCreationExpressionSyntax;

                foreach (var initializer in anonymousExpression.Initializers
                                                               .Where(a => IsInvalidSimpleSelector(context, a.Expression)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(_rule, initializer.GetLocation(), "must be a simple member expression (t.A)"));
                }
            }
            else
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, lambdaExpression.Body.GetLocation(), "somthing something must be t => t.A or t => new { t.A, t.B }"));
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(syntaxContext =>
            {
                var invocationExpression = syntaxContext.Node as InvocationExpressionSyntax;
                var memberAccessExpression = invocationExpression?.Expression as MemberAccessExpressionSyntax;

                var name = (memberAccessExpression?.Name?.Identifier)?.Text;
                if (_methodHooks.ContainsKey(name))
                {
                    var methodSymbol = syntaxContext.SemanticModel.GetSymbolInfo(memberAccessExpression).Symbol as IMethodSymbol;

                    if (methodSymbol?.ToString()?.StartsWith(_methodHooks[name]) == true)
                    {
                        if (name == "Insert")
                        {
                            var columnsSelector = invocationExpression.ArgumentList.Arguments[1];

                            ValidateMultiSelector(syntaxContext, columnsSelector);
                        }
                    }
                }

            }, SyntaxKind.InvocationExpression);
        }
    }
}
