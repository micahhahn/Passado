﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Passado.Core.Model;
using Passado.Analyzers.Model;

namespace Passado.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ModelAnalyzer : DiagnosticAnalyzer
    {
        public static string InvalidTableSelector = "PassadoModelInvalidTableSelector";
        public static string RepeatedTableSelector = "PassadoModelRepeatedTableSelector";
        public static string RepeatedTableName = "PassadoModelRepeatedTableName";
        public static string InvalidColumnSelector = "PassadoModelInvalidColumnSelector";
        public static string RepeatedColumnSelector = "PassadoModelRepeatedColumnSelector";
        public static string RepeatedColumnName = "PassadoModelRepeatedColumnName";
        public static string InvalidSqlType = "PassadoInvalidSqlType";
        public static string InvalidSqlTypeForIdentity = "PassadoInvalidSqlTypeForIdentity";

        static Dictionary<string, DiagnosticDescriptor> _descriptors;

        static ModelAnalyzer()
        {
            var temp = new List<(string Id, string Title, string Message)>()
            {
                (InvalidTableSelector, "Invalid Table Selector", "The table selector must reference a property of the database."),
                (RepeatedTableSelector, "Repeated Table Selector", "A table can only modelled once."),
                (RepeatedTableName, "Repeated Table Name", "Each table name + schema must be unique."),
                (InvalidColumnSelector, "Invalid Column Selector", "The column selector must reference a property of the table."),
                (RepeatedColumnSelector, "Repeated Column Selector", "A column can only be modelled once."),
                (RepeatedColumnName, "Repeated Column Name", "Each column name must be unique."),
                (InvalidSqlType, "Invalid Sql Type", "This sql type is incompatible.  Either change the type or provide a type converter."),
                (InvalidSqlTypeForIdentity, "Invalid Sql Type For Identity", "This column type cannot be an identity.")
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

        static Optional<FuzzyProperty> ParseSimpleProperty(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, string diagnosticId)
        {
            var lambdaExpression = argument.Expression as SimpleLambdaExpressionSyntax;

            if (lambdaExpression == null)
                return new Optional<FuzzyProperty>();

            var selector = lambdaExpression.Body as MemberAccessExpressionSyntax;

            if (selector == null || !(context.SemanticModel.GetSymbolInfo(selector.Expression).Symbol is IParameterSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(_descriptors[diagnosticId], argument.GetLocation()));
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

            var namedArgs = remainingOrder.Select(o => dictionary.TryGetValue(o, out var arg) ? arg : null);

            positionalArguments.AddRange(namedArgs);

            return positionalArguments;
        }

        static Optional<T> ParseConstantArgument<T>(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            if (argument == null)
            {
                return new Optional<T>(default(T));
            }
            else
            {
                var argumentValue = context.SemanticModel.GetConstantValue(argument.Expression);

                return argumentValue.HasValue ? new Optional<T>((T)argumentValue.Value)
                                              : new Optional<T>();
            }
        }

        static FuzzyTableModel ParseTableTable(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression, FuzzyDatabaseModel partialDatabase)
        {
            var firstArgument = expression.ArgumentList.Arguments[0];

            var fuzzyTableModel = new FuzzyTableModel()
            {
                Property = ParseSimpleProperty(context, firstArgument, InvalidTableSelector),
                Columns = new List<FuzzyColumnModel>()
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
                // Use property name
                fuzzyTableModel.Name = fuzzyTableModel.Property.HasValue ? new Optional<string>(fuzzyTableModel.Property.Value?.Name)
                                                                         : new Optional<string>();
            }
            else
            {
                fuzzyTableModel.Name = ParseConstantArgument<string>(context, nameArgument);
            }

            fuzzyTableModel.Schema = ParseConstantArgument<string>(context, schemaArgument);

            if (fuzzyTableModel.Name.HasValue && fuzzyTableModel.Schema.HasValue)
            {
                if (partialDatabase.Tables.Any(t => t.Name.HasValue &&
                                                    t.Schema.HasValue &&
                                                    t.Name.Value == fuzzyTableModel.Name.Value &&
                                                    t.Schema.Value == fuzzyTableModel.Schema.Value))
                    context.ReportDiagnostic(Diagnostic.Create(_descriptors[RepeatedTableName], nameArgument == null ? firstArgument.GetLocation() : nameArgument.GetLocation()));
            }

            return fuzzyTableModel;
        }

        static Dictionary<SpecialType, HashSet<SqlType>> _defaultTypeMappings = new Dictionary<SpecialType, HashSet<SqlType>>()
        {
            { SpecialType.System_Boolean, new HashSet<SqlType>() { SqlType.Bit } },
            { SpecialType.System_Byte, new HashSet<SqlType>() { SqlType.Byte } },
            { SpecialType.System_Int16, new HashSet<SqlType>() { SqlType.Short } },
            { SpecialType.System_Int32, new HashSet<SqlType>() { SqlType.Int } },
            { SpecialType.System_Int64, new HashSet<SqlType>() { SqlType.Long } },
            { SpecialType.System_String, new HashSet<SqlType>() { SqlType.String } },
            { SpecialType.System_DateTime, new HashSet<SqlType>() { SqlType.DateTime, SqlType.Date, SqlType.Time } },
            { SpecialType.System_Single, new HashSet<SqlType>() { SqlType.Single } },
            { SpecialType.System_Double, new HashSet<SqlType>() { SqlType.Double } }
        };

        static FuzzyTableModel ParseTableColumn(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression, FuzzyDatabaseModel partialDatabase)
        {
            (var innerInvocation, var innerMethodName) = PeekChain(expression);

            var innerModel = innerMethodName == "Column" ? ParseTableColumn(context, innerInvocation, partialDatabase) :
                             innerMethodName == "Table" ? ParseTableTable(context, innerInvocation, partialDatabase) :
                             throw new NotImplementedException();

            var selector = expression.ArgumentList.Arguments[0];

            var fuzzyColumnModel = new FuzzyColumnModel()
            {
                Property = ParseSimpleProperty(context, selector, InvalidColumnSelector)
            };
        
            if (fuzzyColumnModel.Property.HasValue)
            {
                if (innerModel.Columns.Any(t => t.Property.HasValue && t.Property.Value.Name == fuzzyColumnModel.Property.Value.Name))
                    context.ReportDiagnostic(Diagnostic.Create(_descriptors[RepeatedColumnSelector], selector.GetLocation()));
            }

            var sqlTypeArg = expression.ArgumentList.Arguments[1];
            var tempSqlType = context.SemanticModel.GetConstantValue(sqlTypeArg.Expression);
            fuzzyColumnModel.Type = tempSqlType.HasValue ? new Optional<SqlType>((SqlType)tempSqlType.Value) : new Optional<SqlType>(); 

            var optionalArgs = RankOptionalArguments(context,
                                                     expression.ArgumentList.Arguments.Skip(2),
                                                     new List<string> { "nullable", "maxLength", "name", "defaultValue", "identity", "converter" });
            
            var nullableArg = optionalArgs[0];
            var maxLengthArg = optionalArgs[1];
            var nameArg = optionalArgs[2];
            var defaultValueArg = optionalArgs[3];
            var identityArg = optionalArgs[4];
            var converterArg = optionalArgs[5];
            
            if (nameArg == null)
            {
                // Use property name
                fuzzyColumnModel.Name = fuzzyColumnModel.Property.HasValue ? new Optional<string>(fuzzyColumnModel.Property.Value?.Name)
                                                                           : new Optional<string>();
            }
            else
            {
                fuzzyColumnModel.Name = ParseConstantArgument<string>(context, nameArg);
            }

            fuzzyColumnModel.MaxLength = ParseConstantArgument<int?>(context, maxLengthArg);
            fuzzyColumnModel.IsIdentity = ParseConstantArgument<bool>(context, identityArg);

            if (fuzzyColumnModel.Name.HasValue)
            {
                if (innerModel.Columns.Any(c => c.Name.HasValue &&
                                                c.Name.Value == fuzzyColumnModel.Name.Value))
                    context.ReportDiagnostic(Diagnostic.Create(_descriptors[RepeatedColumnName], nameArg == null ? selector.GetLocation() : nameArg.GetLocation()));
            }
            
            if (fuzzyColumnModel.Property.HasValue && fuzzyColumnModel.Type.HasValue)
            {
                if (fuzzyColumnModel.Property.Value.Type.TypeKind == TypeKind.Enum)
                {
                    if (!(fuzzyColumnModel.Type.Value == SqlType.String || fuzzyColumnModel.Type.Value == SqlType.Int))
                        context.ReportDiagnostic(Diagnostic.Create(_descriptors[InvalidSqlType], sqlTypeArg.GetLocation()));

                    if (fuzzyColumnModel.Type.Value == SqlType.String && fuzzyColumnModel.MaxLength.HasValue)
                    {
                        var namedType = fuzzyColumnModel.Property.Value.Type as INamedTypeSymbol;
                        var longestEnum = namedType.MemberNames.Max(n => n.Length);

                        if (fuzzyColumnModel.MaxLength.Value != null && 
                            fuzzyColumnModel.MaxLength.Value < longestEnum)
                            context.ReportDiagnostic(Diagnostic.Create(_descriptors[InvalidSqlType], sqlTypeArg.GetLocation()));
                    }
                }
                else if (fuzzyColumnModel.Property.Value.Type.Name == "DateTimeOffset" ||
                         fuzzyColumnModel.Property.Value.Type.Name == "Guid")
                {
                    if ((fuzzyColumnModel.Property.Value.Type.Name == "DateTimeOffset" && fuzzyColumnModel.Type.Value != SqlType.DateTimeOffset) ||
                        (fuzzyColumnModel.Property.Value.Type.Name == "Guid" && fuzzyColumnModel.Type.Value != SqlType.Guid))
                        context.ReportDiagnostic(Diagnostic.Create(_descriptors[InvalidSqlType], sqlTypeArg.GetLocation()));
                }
                else
                {
                    var propertyType = fuzzyColumnModel.Property.Value.Type.SpecialType;
                    
                    if (!_defaultTypeMappings[propertyType].Contains(fuzzyColumnModel.Type.Value))
                        context.ReportDiagnostic(Diagnostic.Create(_descriptors[InvalidSqlType], sqlTypeArg.GetLocation()));
                }
            }
            
            if (fuzzyColumnModel.Type.HasValue &&
                fuzzyColumnModel.IsIdentity.HasValue &&
                fuzzyColumnModel.IsIdentity.Value == true)
            {
                var nonIdentityTypes = new List<SqlType>()
                {
                    SqlType.String,
                    SqlType.Bit,
                    SqlType.Date,
                    SqlType.Time,
                    SqlType.DateTime,
                    SqlType.DateTimeOffset,
                    SqlType.Single,
                    SqlType.Double,
                    SqlType.Guid
                };

                if (nonIdentityTypes.Contains(fuzzyColumnModel.Type.Value))
                    context.ReportDiagnostic(Diagnostic.Create(_descriptors[InvalidSqlTypeForIdentity], identityArg.GetLocation()));
            }

            innerModel.Columns.Add(fuzzyColumnModel);
            
            return innerModel;
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
               Name = ParseConstantArgument<string>(context, nameArgument),
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
