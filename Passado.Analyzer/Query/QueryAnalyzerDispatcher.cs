using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Passado.Analyzers.Query
{
    /// <summary>
    /// Top level DiagnosticAnalyzer that dispatches query invocations (Select, Insert, Update, Delete) to the appropiate analyzers.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class QueryAnalyzerDispatcher : DiagnosticAnalyzer
    {
        static readonly QueryAnalyzerBase[] _queryAnalzyers =
        {

        };

        static readonly QueryMethodAnalyzerBase[] _queryMethodAnalyzers =
        {
            new TableSelectorAnalyzer()
        };

        static readonly Dictionary<string, List<QueryMethodAnalyzerBase>> _queryMethodHooks;

        static QueryAnalyzerDispatcher()
        {
            _queryMethodHooks = new Dictionary<string, List<QueryMethodAnalyzerBase>>();

            foreach (var analyzer in _queryMethodAnalyzers)
            {
                foreach (var methodHook in analyzer.MethodHooks)
                {
                    if (!_queryMethodHooks.ContainsKey(methodHook))
                    {
                        _queryMethodHooks[methodHook] = new List<QueryMethodAnalyzerBase>();
                    }

                    _queryMethodHooks[methodHook].Add(analyzer);
                }
            }
        }
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return _queryAnalzyers.Select(q => q.Rule)
                                     .ToImmutableArray()
                                     .AddRange(_queryMethodAnalyzers.Select(q => q.Rule));
            }
        }

        void AnalyzeInvocationChain(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
        {
            var memberAccessExpression = invocation.Expression as MemberAccessExpressionSyntax;

            var methodName = memberAccessExpression.Name.Identifier.Text;

            // Dispatch here based on method name
            if (_queryMethodHooks.TryGetValue(methodName, out List< QueryMethodAnalyzerBase> methodHooks))
            {
                foreach (var methodHook in methodHooks)
                {
                    methodHook.AnalyzeQueryMethod(context, methodName, invocation.ArgumentList);
                }
            }

            if (memberAccessExpression.Expression is InvocationExpressionSyntax)
            {
                AnalyzeInvocationChain(context, memberAccessExpression.Expression as InvocationExpressionSyntax);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(syntaxContext =>
            {
                var invocationExpression = syntaxContext.Node as InvocationExpressionSyntax;
                var memberAccessExpression = invocationExpression?.Expression as MemberAccessExpressionSyntax;

                var name = (memberAccessExpression?.Name?.Identifier)?.Text;
                if (name == "Select" || name == "Insert" || name == "Update" || name == "Delete")
                {
                    var methodSymbol = syntaxContext.SemanticModel.GetSymbolInfo(memberAccessExpression).Symbol as IMethodSymbol;

                    if (methodSymbol?.ToString()?.StartsWith("Passado.Core.IQueryBuilder") == true)
                    {
                        var query = invocationExpression.ArgumentList.Arguments[0].Expression as SimpleLambdaExpressionSyntax;

                        foreach (var queryAnalyzer in _queryAnalzyers)
                        {
                            queryAnalyzer.AnalyzeQuery(syntaxContext, query);
                        }

                        AnalyzeInvocationChain(syntaxContext, query.Body as InvocationExpressionSyntax);
                    }
                }

            }, SyntaxKind.InvocationExpression);
        }
    }
}
