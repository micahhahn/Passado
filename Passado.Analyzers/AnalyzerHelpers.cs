﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Passado.Analyzers.Model;
using Passado.Model;

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

        public static (SyntaxNode body, ParameterSyntax parameter) DeconstructLambda(ArgumentSyntax argument)
        {
            if (argument.Expression is SimpleLambdaExpressionSyntax simpleLambda)
            {
                return (simpleLambda.Body, simpleLambda.Parameter);
            }
            else if (argument.Expression is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
            {
                return (parenthesizedLambda.Body, parenthesizedLambda.ParameterList.Parameters[0]);
            }

            throw new NotImplementedException();
        }

        //public static Optional<(FuzzyProperty, Location)> ParsePropertyLocation(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, ModelBuilderError error)
        //{
        //    var optional = ParseSelector(context, argument, error, "", false);

        //    if (!optional.HasValue)
        //        return new Optional<(FuzzyProperty, Location)>();

        //    return Just((optional.Value, argument.GetLocation()));
        //}

        public static bool IsNull(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            var constant = context.SemanticModel.GetConstantValue(argument.Expression);
            return constant.HasValue && constant.Value == null;
        }

        public static Optional<FuzzyProperty> ParseSelector(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, ModelBuilderError nullError, ModelBuilderError propertyError, bool isOptional)
        {
            // If the argument is null, this must be an optional property
            if (argument == null)
                return new Optional<FuzzyProperty>(null);
            
            var lambdaExpression = argument.Expression as SimpleLambdaExpressionSyntax;

            if (lambdaExpression == null)
                return new Optional<FuzzyProperty>();

            var selector = lambdaExpression.Body as MemberAccessExpressionSyntax;
            
            if (selector == null || !(context.SemanticModel.GetSymbolInfo(selector.Expression).Symbol is IParameterSymbol))
            {
                context.ReportDiagnostic(propertyError.MakeDiagnostic(argument.GetLocation()));
                return new Optional<FuzzyProperty>(null);
            }
            else
            {
                var propertySymbol = context.SemanticModel.GetSymbolInfo(selector).Symbol as IPropertySymbol;

                return new Optional<FuzzyProperty>(new FuzzyProperty(propertySymbol));
            }
        }

        static FuzzyProperty ParseMemberAccessProperty(SyntaxNodeAnalysisContext context, MemberAccessExpressionSyntax memberAccess)
        {
            //if (!(context.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is IParameterSymbol))
            //    throw new NotImplementedException();

            var propertySymbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol as IPropertySymbol;

            return new FuzzyProperty(propertySymbol);
        }

        public static Optional<ImmutableArray<(FuzzyProperty, Location)>> ParseMultiPropertySelector(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            (FuzzyProperty, Location) ParsePropertySelector(ExpressionSyntax expression)
            {
                if (expression is MemberAccessExpressionSyntax)
                {
                    return (ParseMemberAccessProperty(context, expression as MemberAccessExpressionSyntax), expression.GetLocation());
                }

                context.ReportDiagnostic(ModelBuilderError.SelectorInvalid("a").MakeDiagnostic(expression.GetLocation()));
                return (null, expression.GetLocation());
            }

            if (argument.Expression is SimpleLambdaExpressionSyntax)
            {
                var lambdaBody = (argument.Expression as SimpleLambdaExpressionSyntax).Body;

                if (lambdaBody is AnonymousObjectCreationExpressionSyntax)
                {
                    var anonymousObject = lambdaBody as AnonymousObjectCreationExpressionSyntax;

                    var properties = anonymousObject.Initializers
                                                    .Select(i => ParsePropertySelector(i.Expression))
                                                    .ToImmutableArray();

                    if (properties.Any(p => p.Item1 == null))
                        return new Optional<ImmutableArray<(FuzzyProperty, Location)>>();
                    else
                        return Just(properties);
                }
                else if (lambdaBody is MemberAccessExpressionSyntax)
                {
                    var property = ParsePropertySelector(lambdaBody as ExpressionSyntax);

                    if (property.Item1 == null)
                        return new Optional<ImmutableArray<(FuzzyProperty, Location)>>();
                    else
                        return Just(ImmutableArray.Create(property));
                }

                context.ReportDiagnostic(ModelBuilderError.MultiSelectorInvalid((argument.Expression as SimpleLambdaExpressionSyntax).Parameter.Identifier.Text).MakeDiagnostic(argument.GetLocation()));
            }

            return new Optional<ImmutableArray<(FuzzyProperty, Location)>>();
        }

        public static Optional<List<FuzzyColumnModel>> ParseMultiColumn(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, List<FuzzyColumnModel> columns)
        {
            var optionalMultiProperty = ParseMultiPropertySelector(context, argument);

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

        public static Optional<ImmutableArray<(SortOrder, FuzzyProperty, Location)>> ParseOrderedMultiProperty(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            if (argument.Expression is LambdaExpressionSyntax)
            {
                (var body, var param) = DeconstructLambda(argument);

                (SortOrder, FuzzyProperty, Location)? ParseOrderedProperty(ExpressionSyntax expression)
                {
                    if (expression is MemberAccessExpressionSyntax propertyExpression &&
                        propertyExpression.Expression is MemberAccessExpressionSyntax orderExpression &&
                        context.SemanticModel.GetSymbolInfo(orderExpression.Expression).Symbol is IParameterSymbol)
                    {
                        var sortOrder = orderExpression.Name.Identifier.Text == "Asc" ? SortOrder.Ascending :
                                        orderExpression.Name.Identifier.Text == "Desc" ? SortOrder.Descending :
                                        throw new NotImplementedException();

                        var property = context.SemanticModel.GetSymbolInfo(propertyExpression).Symbol as IPropertySymbol;

                        return (sortOrder, new FuzzyProperty(property), expression.GetLocation());
                    }

                    context.ReportDiagnostic(ModelBuilderError.OrderedSelectorInvalid(param.Identifier.Text).MakeDiagnostic(expression.GetLocation()));
                    return null;
                }

                var properties = body is AnonymousObjectCreationExpressionSyntax anonymousExpression ? anonymousExpression.Initializers.Select(i => i.Expression).ToList() :
                                 body is MemberAccessExpressionSyntax memberExpression ? new List<ExpressionSyntax>() { memberExpression } :
                                 null as List<ExpressionSyntax>;

                if (properties != null)
                {
                    var orderedProperties = properties.Select(p => ParseOrderedProperty(p));

                    if (orderedProperties.Any(p => !p.HasValue))
                        return new Optional<ImmutableArray<(SortOrder, FuzzyProperty, Location)>>();

                    return Just(orderedProperties.Select(p => p.Value).ToImmutableArray());
                }

                context.ReportDiagnostic(ModelBuilderError.OrderedMultiSelectorInvalid(param.Identifier.Text).MakeDiagnostic(body.GetLocation()));
            }

            return new Optional<ImmutableArray<(SortOrder, FuzzyProperty, Location)>>();
        }

        public static Optional<ImmutableArray<(SortOrder, FuzzyColumnModel)>> MatchColumns(SyntaxNodeAnalysisContext context, IEnumerable<(SortOrder, FuzzyProperty, Location)> properties, IEnumerable<FuzzyColumnModel> columns, string tableName)
        {
            return Just(properties.Select(p =>
            {
                var column = columns.FirstOrDefault(c => c.Property.HasValue && c.Property.Value.Name == p.Item2.Name);

                if (column == null)
                {
                    // We can only say that this column is not modeled if there are no indeterminate columns
                    if (columns.All(c => c.Property.HasValue))
                        context.ReportDiagnostic(ModelBuilderError.SelectorNotMappedToColumn(p.Item2.Name, tableName).MakeDiagnostic(p.Item3));
                }

                return (p.Item1, column);
            }).ToImmutableArray());
        }
    }
}
