using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Passado.Analyzers.Model;
using Passado.Core.Model;

namespace Passado.Analyzers
{
    public static class AnalyzerHelpers
    {
        public static Optional<T> Just<T>(T arg)
        {
            return new Optional<T>(arg);
        }

        /// <summary>
        /// Peeks the previous method in the call chain returning the prior invocation and method name.
        /// </summary>
        /// <param name="expression">The current invocation.</param>
        /// <returns>The prior invocation and prior method name.</returns>
        public static (InvocationExpressionSyntax, string) PeekChain(InvocationExpressionSyntax expression)
        {
            var innerInvocation = (expression.Expression as MemberAccessExpressionSyntax)
                                ?.Expression as InvocationExpressionSyntax;

            var innerMethodName = ((innerInvocation.Expression as MemberAccessExpressionSyntax)
                                ?.Name?.Identifier)?.Text;

            return (innerInvocation, innerMethodName);
        }

        /// <summary>
        /// Parses the arguments of an invocation expression and returns them in a dictionary keyed by name.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="expression">The invocation expression.</param>
        /// <returns>A dictionary mapping every parameter name to its argument syntax.  If a parameter is not specified then the argument syntax is null.</returns>
        public static Dictionary<string, ArgumentSyntax> ParseArguments(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression)
        {
            var parameters = (context.SemanticModel.GetSymbolInfo(expression.Expression).Symbol as IMethodSymbol).Parameters;
            var arguments = expression.ArgumentList.Arguments;

            var dictionary = arguments.Zip(parameters, (a, p) => (a, p))
                                      .TakeWhile(z => z.Item1.NameColon == null)
                                      .ToDictionary(z => z.Item2.Name, z => z.Item1);

            var remainingArgs = arguments.Skip(dictionary.Count);

            foreach (var param in parameters.Skip(dictionary.Count))
            {
                dictionary.Add(param.Name, remainingArgs.SingleOrDefault(a => a.NameColon.Name.Identifier.Text == param.Name));
            }

            return dictionary;
        }

        public static Optional<T> ParseConstantArgument<T>(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, Func<Optional<T>> defaultValue)
        {
            if (argument == null)
            {
                return defaultValue();
            }
            else
            {
                var argumentValue = context.SemanticModel.GetConstantValue(argument.Expression);

                return argumentValue.HasValue ? new Optional<T>((T)argumentValue.Value)
                                              : new Optional<T>();
            }
        }
        
        public static Optional<(FuzzyProperty, Location)> ParsePropertyLocation(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            var optional = ParseProperty(context, argument);

            if (!optional.HasValue)
                return new Optional<(FuzzyProperty, Location)>();

            return Just((optional.Value, argument.GetLocation()));
        }

        public static Optional<FuzzyProperty> ParseProperty(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            var lambdaExpression = argument.Expression as SimpleLambdaExpressionSyntax;

            if (lambdaExpression == null)
                return new Optional<FuzzyProperty>();

            var selector = lambdaExpression.Body as MemberAccessExpressionSyntax;

            if (selector == null || !(context.SemanticModel.GetSymbolInfo(selector.Expression).Symbol is IParameterSymbol))
            {
                throw new NotImplementedException();
                //context.ReportDiagnostic(Diagnostic.Create(_descriptors[diagnosticId], argument.GetLocation()));
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

        static FuzzyProperty ParseMemberAccessProperty(SyntaxNodeAnalysisContext context, MemberAccessExpressionSyntax memberAccess)
        {
            if (!(context.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is IParameterSymbol))
                throw new NotImplementedException();

            var propertySymbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol as IPropertySymbol;

            return new FuzzyProperty()
            {
                Name = propertySymbol.Name,
                Type = propertySymbol.Type
            };
        }

        public static Optional<List<(FuzzyProperty, Location)>> ParseMultiProperty(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            (FuzzyProperty, Location) ParseProperty(ExpressionSyntax expression)
            {
                if (expression is MemberAccessExpressionSyntax)
                {
                    return (ParseMemberAccessProperty(context, expression as MemberAccessExpressionSyntax), expression.GetLocation());
                }

                throw new NotImplementedException();
            }

            if (argument.Expression is SimpleLambdaExpressionSyntax)
            {
                var lambdaBody = (argument.Expression as SimpleLambdaExpressionSyntax).Body;

                if (lambdaBody is AnonymousObjectCreationExpressionSyntax)
                {
                    var anonymousObject = lambdaBody as AnonymousObjectCreationExpressionSyntax;

                    return Just(anonymousObject.Initializers
                                                       .Select(i => ParseProperty(i.Expression))
                                                       .ToList());
                }
                else if (lambdaBody is MemberAccessExpressionSyntax)
                {
                    return Just(new List<(FuzzyProperty, Location)>() { ParseProperty(lambdaBody as ExpressionSyntax) });
                }

                throw new NotImplementedException();
            }

            return new Optional<List<(FuzzyProperty, Location)>>();
        }

        public static Optional<List<FuzzyColumnModel>> ParseMultiColumn(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, List<FuzzyColumnModel> columns)
        {
            var optionalMultiProperty = ParseMultiProperty(context, argument);

            if (!optionalMultiProperty.HasValue)
                return new Optional<List<FuzzyColumnModel>>();
            
            return Just(optionalMultiProperty.Value.Select(p =>
            {
                var column = columns.FirstOrDefault(c => c.Property.HasValue && c.Property.Value.Name == p.Item1.Name);

                if (column == null)
                {
                    // We can only say that this column is not modeled if there are no indeterminate columns
                    if (columns.All(c => c.Property.HasValue))
                        throw new NotImplementedException();
                }

                return column;
            }).ToList());
        }

        public static Optional<List<(SortOrder, FuzzyProperty, Location)>> ParseOrderedMultiProperty(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            (SortOrder, FuzzyProperty, Location) ParseOrderedProperty(ExpressionSyntax expression)
            {
                var sortOrder = SortOrder.Ascending;

                if (expression is CastExpressionSyntax)
                {
                    var castExpression = expression as CastExpressionSyntax;

                    var typeName = ((castExpression.Type as IdentifierNameSyntax)?.Identifier)?.Text;

                    if (typeName == nameof(Asc))
                        sortOrder = SortOrder.Ascending;
                    else if (typeName == nameof(Desc))
                        sortOrder = SortOrder.Descending;
                    else
                        throw new NotImplementedException();

                    expression = castExpression.Expression;
                }

                if (expression is MemberAccessExpressionSyntax)
                {
                    return (sortOrder, ParseMemberAccessProperty(context, expression as MemberAccessExpressionSyntax), expression.GetLocation());
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (argument.Expression is SimpleLambdaExpressionSyntax)
            {
                var lambdaBody = (argument.Expression as SimpleLambdaExpressionSyntax).Body;

                if (lambdaBody is AnonymousObjectCreationExpressionSyntax)
                {
                    var anonymousObject = lambdaBody as AnonymousObjectCreationExpressionSyntax;

                    return Just(anonymousObject.Initializers
                                                       .Select(i => ParseOrderedProperty(i.Expression))
                                                       .ToList());
                }
                else if (lambdaBody is MemberAccessExpressionSyntax)
                {
                    return Just(new List<(SortOrder, FuzzyProperty, Location)>() { ParseOrderedProperty(lambdaBody as ExpressionSyntax) });
                }

                throw new NotImplementedException();
            }

            return new Optional<List<(SortOrder, FuzzyProperty, Location)>>();
        }

        public static Optional<List<(SortOrder, FuzzyColumnModel)>> ParseOrderedMultiColumn(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, List<FuzzyColumnModel> columns)
        {
            var optionalOrderedMultiProperty = ParseOrderedMultiProperty(context, argument);

            if (!optionalOrderedMultiProperty.HasValue)
                return new Optional<List<(SortOrder, FuzzyColumnModel)>>();

            return Just(optionalOrderedMultiProperty.Value.Select(p =>
            {
                var column = columns.FirstOrDefault(c => c.Property.HasValue && c.Property.Value.Name == p.Item2.Name);

                if (column == null)
                {
                    // We can only say that this column is not modeled if there are no indeterminate columns
                    if (columns.All(c => c.Property.HasValue))
                        throw new NotImplementedException();
                }

                return (p.Item1, column);
            }).ToList());

            throw new NotImplementedException();
        }
    }
}
