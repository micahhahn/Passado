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
    public class SimpleSelectorAnalyzer : DiagnosticAnalyzer
    {
        public static string Id => "PassadoSimpleSelectorAnalyzer";

        static DiagnosticDescriptor _rule = new DiagnosticDescriptor(id: Id,
                                                                     title: "This is a title.",
                                                                     messageFormat: "{0}",
                                                                     category: "Passado",
                                                                     defaultSeverity: DiagnosticSeverity.Error,
                                                                     isEnabledByDefault: true);

        static Dictionary<string, string> _methodHooks = new Dictionary<string, string>()
        {
            { "Table",     "Passado.Core.Model.Builder.TableModelBuilder" },
            { "Column",    "Passado.Core.Model.Builder.ColumnModelBuilderBase" },
            { "From",      "Passado.Core.IQueryBuilder" },
            { "Insert",    "Passado.Core.IQueryBuilder" },
            { "Update",    "Passado.Core.IQueryBuilder" },
            { "Delete",    "Passado.Core.IQueryBuilder" },
            { "Join",      "Passado.Core.Query" },
            { "LeftJoin",  "Passado.Core.Query" },
            { "RightJoin", "Passado.Core.Query" },
            { "OuterJoin", "Passado.Core.Query" },
            { "Set",       "Passado.Core.Query.Update.ISetable" }     
        };
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public static bool IsInvalidSimpleSelector(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            var lambdaExpression = argument.Expression as SimpleLambdaExpressionSyntax;

            // If it's not a lambda expression, we can't easily do any compile time analysis
            if (lambdaExpression == null)
                return false;

            var selector = lambdaExpression.Body as MemberAccessExpressionSyntax;

            if (selector == null)
                return true;

            return !(context.SemanticModel.GetSymbolInfo(selector.Expression).Symbol is IParameterSymbol);
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
                        var firstArgument = invocationExpression.ArgumentList.Arguments[0];
                        if (IsInvalidSimpleSelector(syntaxContext, firstArgument))
                        {
                            syntaxContext.ReportDiagnostic(Diagnostic.Create(_rule, (firstArgument.Expression as SimpleLambdaExpressionSyntax).Body.GetLocation(), ""));
                        }
                    }
                }

            }, SyntaxKind.InvocationExpression);
        }
    }
}
