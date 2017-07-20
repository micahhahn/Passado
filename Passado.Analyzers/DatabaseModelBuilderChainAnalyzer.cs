using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;

using Passado.Analyzers.Model;

namespace Passado.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DatabaseModelBuilderChainAnalyzer : DiagnosticAnalyzer
    {
        public static string InvalidTableSelector = "PassadoModelInvalidTableSelector";
        public static string RepeatedTableSelector = "PassadoModelRepeatedTableSelector";
        public static string RepeatedTableName = "PassadoModelRepeatedTableName";

        static Dictionary<string, DiagnosticDescriptor> _descriptors;

        static DatabaseModelBuilderChainAnalyzer()
        {
            var temp = new List<(string Id, string Title, string Message)>()
            {
                (InvalidTableSelector, "Invalid Table Selector", "The table selector must reference a property of the database."),
                (RepeatedTableSelector, "Repeated Table Selector", "A table can only modelled once."),
                (RepeatedTableName, "Repeated Table Name", "Each table name must be unique.")
            };

            _descriptors = temp.Select(t => new DiagnosticDescriptor(t.Id, t.Title, t.Message, "Passado", DiagnosticSeverity.Error, true))
                               .ToDictionary(t => t.Id);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _descriptors.Values.ToImmutableArray();

        static (InvocationExpressionSyntax, string) PeekChain(InvocationExpressionSyntax expression)
        {
            var innerInvocation = (expression.Expression as MemberAccessExpressionSyntax)
                                ?.Expression as InvocationExpressionSyntax;

            var innerMethodName = ((innerInvocation.Expression as MemberAccessExpressionSyntax)
                                ?.Name?.Identifier)?.Text;

            return (innerInvocation, innerMethodName);
        }

        static Optional<FuzzyProperty> ParseSimpleProperty(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            var lambdaExpression = argument.Expression as SimpleLambdaExpressionSyntax;

            if (lambdaExpression == null)
                return new Optional<FuzzyProperty>();

            var selector = lambdaExpression.Body as MemberAccessExpressionSyntax;

            if (selector == null || !(context.SemanticModel.GetSymbolInfo(selector.Expression).Symbol is IParameterSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(_descriptors[InvalidTableSelector], argument.GetLocation()));
                return new Optional<FuzzyProperty>(null);
            }
            else
            {
                var propertySymbol = context.SemanticModel.GetSymbolInfo(selector).Symbol as IPropertySymbol;

                return new Optional<FuzzyProperty>(new FuzzyProperty()
                {
                    Name = propertySymbol.Name,
                    Type = propertySymbol.Type
                });
            }
        }

        static List<ArgumentSyntax> RankOptionalArguments(SyntaxNodeAnalysisContext context, IEnumerable<ArgumentSyntax> arguments, IEnumerable<string> order)
        {
            var positionalArguments = arguments.Where(a => a.NameColon == null)
                                               .ToList();
            
            var remainingArgs = arguments.Skip(positionalArguments.Count);
            var remainingOrder = order.Skip(positionalArguments.Count);

            var dictionary = remainingArgs.ToDictionary(a => a.NameColon.Name.Identifier.Text);

            var namedArgs = order.Select(o => dictionary.TryGetValue(o, out var arg) ? arg : null);

            positionalArguments.AddRange(namedArgs);

            return positionalArguments;
        }

        static Optional<string> ParseStringArgument(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            if (argument == null)
            {
                return new Optional<string>();
            }
            else
            {
                var argumentValue = context.SemanticModel.GetConstantValue(argument.Expression);

                return argumentValue.HasValue ? new Optional<string>(argumentValue.Value as string)
                                       : new Optional<string>();
            }
        }

        static FuzzyTableModel ParseTableTable(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression, FuzzyDatabaseModel partialDatabase)
        {
            var firstArgument = expression.ArgumentList.Arguments[0];

            var fuzzyTableModel = new FuzzyTableModel()
            {
                Property = ParseSimpleProperty(context, firstArgument)
            };
            
            if (fuzzyTableModel.Property.HasValue)
            {
                if (partialDatabase.Tables.Any(t => t.Property.HasValue && t.Property.Value.Name == fuzzyTableModel.Property.Value.Name))
                    context.ReportDiagnostic(Diagnostic.Create(_descriptors[RepeatedTableSelector], expression.ArgumentList.Arguments[0].GetLocation()));
            }

            var optionalArgs = RankOptionalArguments(context, expression.ArgumentList.Arguments.Skip(1), new List<string>() { "name", "schema" });

            var nameArgument = optionalArgs[0];
            var schemaArgument = optionalArgs[1];

            if (nameArgument == null)
            {
                // If name not specified, then we use the property name
                fuzzyTableModel.Name = fuzzyTableModel.Property.HasValue ? new Optional<string>(fuzzyTableModel.Property.Value?.Name)
                                                                         : new Optional<string>();
            }
            else
            {
                fuzzyTableModel.Name = ParseStringArgument(context, nameArgument);
            }

            if (fuzzyTableModel.Name.HasValue)
            {
                if (partialDatabase.Tables.Any(t => t.Name.HasValue && t.Name.Value == fuzzyTableModel.Name.Value))
                    context.ReportDiagnostic(Diagnostic.Create(_descriptors[RepeatedTableName], nameArgument == null ? firstArgument.GetLocation() : nameArgument.GetLocation()));
            }

            fuzzyTableModel.Schema = ParseStringArgument(context, schemaArgument);

            return fuzzyTableModel;
        }

        static FuzzyTableModel ParseTableColumn(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression, FuzzyDatabaseModel partialDatabase)
        {
            (var innerInvocation, var innerMethodName) = PeekChain(expression);

            if (innerMethodName == "Column")
                return ParseTableColumn(context, innerInvocation, partialDatabase);
            else if (innerMethodName == "Table")
                return ParseTableTable(context, innerInvocation, partialDatabase);
            else
                throw new NotImplementedException();
        }

        static FuzzyTableModel ParseTablePrimaryKey(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression, FuzzyDatabaseModel partialDatabase)
        {
            (var innerInvocation, var innerMethodName) = PeekChain(expression);

            if (innerMethodName == "Column")
                return ParseTableColumn(context, innerInvocation, partialDatabase);
            else
                throw new NotImplementedException();
        }

        static FuzzyTableModel ParseTableBuild(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression, FuzzyDatabaseModel partialDatabase)
        {
            (var innerInvocation, var innerMethodName) = PeekChain(expression);

            if (innerMethodName == "PrimaryKey")
                return ParseTablePrimaryKey(context, innerInvocation, partialDatabase);
            else
                throw new NotImplementedException();
        }

        static FuzzyDatabaseModel ParseDatabase(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression)
        {
            var nameArgument = expression.ArgumentList.Arguments[0];
            
            return new FuzzyDatabaseModel()
            {
               Name = ParseStringArgument(context, nameArgument),
               Tables = new List<FuzzyTableModel>()
            };
        }

        static FuzzyDatabaseModel ParseTable(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression)
        {
            (var innerInvocation, var innerMethodName) = PeekChain(expression);

            var innerModel = null as FuzzyDatabaseModel;

            if (innerMethodName == "Table")
                innerModel = ParseTable(context, innerInvocation);
            else if (innerMethodName == "Database")
                innerModel = ParseDatabase(context, innerInvocation);
            else
                throw new NotImplementedException();

            var firstArgument = expression.ArgumentList.Arguments[0];

            if (firstArgument.Expression is MemberAccessExpressionSyntax)
            {
                var _ = context.SemanticModel.GetSymbolInfo(firstArgument.Expression);
                
                // Static class function reference (e.g. User.ProvideModel)
                var staticFunctionMember = firstArgument.Expression as MemberAccessExpressionSyntax;
                var className = (staticFunctionMember.Expression as IdentifierNameSyntax).Identifier.Text;
                var functionName = (staticFunctionMember?.Name?.Identifier)?.Text;

                throw new NotImplementedException();
            }
            else if (firstArgument.Expression is SimpleLambdaExpressionSyntax)
            {
                // Defined inline
                //ParseTableChain(context, firstArgument.Expression);
                var lambdaExpression = firstArgument.Expression as SimpleLambdaExpressionSyntax;

                var table = ParseTableBuild(context, lambdaExpression.Body as InvocationExpressionSyntax, innerModel);

                innerModel.Tables.Add(table);
            }

            return innerModel;
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(syntaxContext =>
            {
                var invocationExpression = syntaxContext.Node as InvocationExpressionSyntax;
                var memberAccessExpression = invocationExpression?.Expression as MemberAccessExpressionSyntax;

                var name = (memberAccessExpression?.Name?.Identifier)?.Text;

                if (name == "Build")
                {
                    var methodSymbol = syntaxContext.SemanticModel.GetSymbolInfo(memberAccessExpression).Symbol as IMethodSymbol;

                    if (methodSymbol?.ToString()?.StartsWith("Passado.Core.Model.Builder.DatabaseTableModelBuilder") == true)
                    {
                        ParseTable(syntaxContext, memberAccessExpression.Expression as InvocationExpressionSyntax);
                    }
                }

            }, SyntaxKind.InvocationExpression);
        }
    }
}
