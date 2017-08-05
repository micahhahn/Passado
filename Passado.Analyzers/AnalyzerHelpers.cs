using System;
using System.Collections.Generic;
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
        public static string InvalidSelector                     = "PS1001";
        public static string InvalidMultiPropertySelector        = "PS1002";
        public static string InvalidOrderedMultiPropertySelector = "PS1003";

        public static List<(string Id, string Title, string Message)> DescriptorsTemp = new List<(string Id, string Title, string Message)>()
        {
            (InvalidSelector, "Invalid Selector", "A {0} selector must be a property of the parameter.  E.g. t => t.A"),
            (InvalidMultiPropertySelector, "Invalid Multi Property Selector", "A {0} selector must be a multi property selector. E.g. t => t.A or t => new { t.A, t.B }"),
            (InvalidOrderedMultiPropertySelector, "Invalid Ordered Multi Property Selector", "A {0} selector must be a multi property selector with optional orderings.")
        };

        public static Dictionary<string, DiagnosticDescriptor> Descriptors = DescriptorsTemp.Select(t => new DiagnosticDescriptor(t.Id, t.Title, t.Message, "Passado", DiagnosticSeverity.Error, true))
                                                                                            .ToDictionary(t => t.Id);

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
        
        //public static Optional<(FuzzyProperty, Location)> ParsePropertyLocation(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, ModelBuilderError error)
        //{
        //    var optional = ParseSelector(context, argument, error, "", false);

        //    if (!optional.HasValue)
        //        return new Optional<(FuzzyProperty, Location)>();

        //    return Just((optional.Value, argument.GetLocation()));
        //}

        public static Optional<FuzzyProperty> ParseSelector(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, ModelBuilderError nullError, ModelBuilderError propertyError, bool isOptional)
        {
            // Check if argument is null.  
            var constant = context.SemanticModel.GetConstantValue(argument.Expression);
            if (constant.HasValue && constant.Value == null)
            {
                if (isOptional)
                {
                    return new Optional<FuzzyProperty>(null);
                }
                else
                {
                    context.ReportDiagnostic(nullError.MakeDiagnostic(argument.GetLocation()));
                    return new Optional<FuzzyProperty>();
                }
            }

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
            if (argument == null)
                return new Optional<List<FuzzyColumnModel>>(null);

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

        public static Optional<List<(SortOrder, FuzzyProperty, Location)>> ParseOrderedMultiProperty(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, ModelBuilderError nullError)
        {
            (SortOrder, FuzzyProperty, Location) ParseOrderedProperty(ExpressionSyntax expression)
            {
                var sortOrder = SortOrder.Ascending;

                if (expression is CastExpressionSyntax)
                {
                    var castExpression = expression as CastExpressionSyntax;

                    var typeName = ((castExpression.Type as IdentifierNameSyntax)?.Identifier)?.Text;

                    //if (typeName == nameof(Asc))
                    //    sortOrder = SortOrder.Ascending;
                    //else if (typeName == nameof(Desc))
                    //    sortOrder = SortOrder.Descending;
                    //else
                    //    throw new NotImplementedException();

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

            // Parse null constant
            var constant = context.SemanticModel.GetConstantValue(argument.Expression);
            if (constant.HasValue && constant.Value == null)
            {
                context.ReportDiagnostic(nullError.MakeDiagnostic(argument.GetLocation()));
            }
            else if (argument.Expression is SimpleLambdaExpressionSyntax)
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

        public static Optional<List<(SortOrder, FuzzyColumnModel)>> ParseOrderedMultiColumn(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, List<FuzzyColumnModel> columns, ModelBuilderError nullError)
        {
            var optionalOrderedMultiProperty = ParseOrderedMultiProperty(context, argument, nullError);

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
