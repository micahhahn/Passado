using System;
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
        public static string InvalidOrderedMultiSelector = "PassadoInvalidOrderedMultiSelector";
        public static string InvalidOrderedSelectorCastType = "InvalidOrderedSelectorCastType";
        public static string MultipleClusteredIndicies = "MultipleClusteredIndicies";

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
                (InvalidSqlTypeForIdentity, "Invalid Sql Type For Identity", "This column type cannot be an identity."),
                (InvalidOrderedMultiSelector, "Invalid Ordered Multi Selector", ""),
                (InvalidOrderedSelectorCastType, "Invalid Ordered Multi Selector Cast Type", "Only 'Asc' and 'Desc' are valid casts to determine ordering."),
                (MultipleClusteredIndicies, "Multiple Clustered Indicies", "Only one index or primary key can be clustered.")
            };

            _descriptors = temp.Select(t => new DiagnosticDescriptor(t.Id, t.Title, t.Message, "Passado", DiagnosticSeverity.Error, true))
                               .ToDictionary(t => t.Id);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _descriptors.Values.ToImmutableArray();

        static string BuildDefaultKeyName(string prefix, string schema, string tableName, IEnumerable<string> columns)
        {
            return $"{prefix}_{(schema != null ? $"{schema}_" : "")}{tableName}__{string.Join("_", columns)}";
        }

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
        
        static Optional<List<FuzzyColumnModel>> ParseMultiProperty(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, List<FuzzyColumnModel> columns)
        {
            throw new NotImplementedException();
        }

        static Optional<List<(SortOrder, FuzzyColumnModel)>> ParseOrderedMultiProperty(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, string diagnosticId, List<FuzzyColumnModel> columns)
        {
            (SortOrder, FuzzyColumnModel) ParseOrderedColumn(ExpressionSyntax expression)
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
                        context.ReportDiagnostic(Diagnostic.Create(_descriptors[InvalidOrderedSelectorCastType], castExpression.Type.GetLocation()));

                    expression = castExpression.Expression;
                }

                if (expression is MemberAccessExpressionSyntax)
                {
                    var memberAccess = expression as MemberAccessExpressionSyntax;

                    if (!(context.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is IParameterSymbol))
                        throw new NotImplementedException();

                    var propertyName = memberAccess.Name.Identifier.Text;

                    var column = columns.FirstOrDefault(c => c.Property.HasValue && c.Property.Value.Name == propertyName);

                    if (column == null)
                    {
                        // We can only say that this column is not modeled if there are no indeterminate columns
                        if (columns.All(c => c.Property.HasValue))
                            throw new NotImplementedException();
                    }
                    
                    return (sortOrder, column);
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

                    return anonymousObject.Initializers.Select(i => ParseOrderedColumn(i.Expression)).ToList();
                }
                else if (lambdaBody is CastExpressionSyntax || lambdaBody is MemberAccessExpressionSyntax)
                {
                    return new List<(SortOrder, FuzzyColumnModel)>() { ParseOrderedColumn(lambdaBody as ExpressionSyntax) };
                }

                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        static Dictionary<string, ArgumentSyntax> ParseArguments(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression)
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

        static Optional<T> ParseConstantArgument<T>(SyntaxNodeAnalysisContext context, ArgumentSyntax argument, T defaultValue)
        {
            if (argument == null)
            {
                return new Optional<T>(defaultValue);
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
            var arguments = ParseArguments(context, expression);

            var tableArg = arguments["table"];
            var nameArg = arguments["name"];
            var schemaArg = arguments["schema"];

            var fuzzyTableModel = new FuzzyTableModel()
            {
                Property = ParseSimpleProperty(context, tableArg, InvalidTableSelector),
                Columns = new List<FuzzyColumnModel>()
            };

            if (fuzzyTableModel.Property.HasValue)
            {
                if (partialDatabase.Tables.Any(t => t.Property.HasValue && t.Property.Value.Name == fuzzyTableModel.Property.Value.Name))
                    context.ReportDiagnostic(Diagnostic.Create(_descriptors[RepeatedTableSelector], expression.ArgumentList.Arguments[0].GetLocation()));
            }
            
            if (nameArg == null)
            {
                // Use property name
                fuzzyTableModel.Name = fuzzyTableModel.Property.HasValue ? new Optional<string>(fuzzyTableModel.Property.Value?.Name)
                                                                         : new Optional<string>();
            }
            else
            {
                fuzzyTableModel.Name = ParseConstantArgument<string>(context, nameArg, null);
            }

            fuzzyTableModel.Schema = ParseConstantArgument<string>(context, schemaArg, null);

            if (fuzzyTableModel.Name.HasValue && fuzzyTableModel.Schema.HasValue)
            {
                if (partialDatabase.Tables.Any(t => t.Name.HasValue &&
                                                    t.Schema.HasValue &&
                                                    t.Name.Value == fuzzyTableModel.Name.Value &&
                                                    t.Schema.Value == fuzzyTableModel.Schema.Value))
                    context.ReportDiagnostic(Diagnostic.Create(_descriptors[RepeatedTableName], nameArg == null ? tableArg.GetLocation() : nameArg.GetLocation()));
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

            var arguments = ParseArguments(context, expression);
            var columnArg = arguments["column"];
            var sqlTypeArg = arguments["type"];
            var nullableArg = arguments["nullable"];
            var maxLengthArg = arguments["maxLength"];
            var nameArg = arguments["name"];
            var defaultValueArg = arguments["defaultValue"];
            var identityArg = arguments["identity"];
            var converterArg = arguments["converter"];
            
            var fuzzyColumnModel = new FuzzyColumnModel()
            {
                Property = ParseSimpleProperty(context, columnArg, InvalidColumnSelector)
            };
        
            if (fuzzyColumnModel.Property.HasValue)
            {
                if (innerModel.Columns.Any(t => t.Property.HasValue && t.Property.Value.Name == fuzzyColumnModel.Property.Value.Name))
                    context.ReportDiagnostic(Diagnostic.Create(_descriptors[RepeatedColumnSelector], columnArg.GetLocation()));
            }

            
            var tempSqlType = context.SemanticModel.GetConstantValue(sqlTypeArg.Expression);
            fuzzyColumnModel.Type = tempSqlType.HasValue ? new Optional<SqlType>((SqlType)tempSqlType.Value) : new Optional<SqlType>(); 
            
            if (nameArg == null)
            {
                // Use property name
                fuzzyColumnModel.Name = fuzzyColumnModel.Property.HasValue ? new Optional<string>(fuzzyColumnModel.Property.Value?.Name)
                                                                           : new Optional<string>();
            }
            else
            {
                fuzzyColumnModel.Name = ParseConstantArgument<string>(context, nameArg, null);
            }

            fuzzyColumnModel.MaxLength = ParseConstantArgument<int?>(context, maxLengthArg, null);
            fuzzyColumnModel.IsIdentity = ParseConstantArgument<bool>(context, identityArg, false);

            if (fuzzyColumnModel.Name.HasValue)
            {
                if (innerModel.Columns.Any(c => c.Name.HasValue &&
                                                c.Name.Value == fuzzyColumnModel.Name.Value))
                    context.ReportDiagnostic(Diagnostic.Create(_descriptors[RepeatedColumnName], nameArg == null ? columnArg.GetLocation() : nameArg.GetLocation()));
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

            var innerModel = (innerMethodName == "Column") ? ParseTableColumn(context, innerInvocation, partialDatabase) :
                             throw new NotImplementedException();

            var arguments = ParseArguments(context, expression);

            var fuzzyPrimaryKey = new FuzzyPrimaryKeyModel()
            {
                Columns = ParseOrderedMultiProperty(context, arguments["keyColumns"], "", innerModel.Columns)
            };
            
            var nameArg = arguments["name"];
            var clusteredArg = arguments["clustered"];

            if (nameArg == null)
            {
                // If the columns are indeterminate or any of the column names are indeterminate or the table name or 
                if (!innerModel.Name.HasValue ||
                    !innerModel.Schema.HasValue ||
                    !fuzzyPrimaryKey.Columns.HasValue ||
                    fuzzyPrimaryKey.Columns.Value.Any(c => !c.Item2.Name.HasValue))
                {
                    fuzzyPrimaryKey.Name = new Optional<string>();
                }
                else
                {
                    fuzzyPrimaryKey.Name = new Optional<string>(BuildDefaultKeyName("PK", innerModel.Schema.Value, innerModel.Name.Value, fuzzyPrimaryKey.Columns.Value.Select(c => c.Item2.Name.Value)));
                }
            }
            else
            {
                fuzzyPrimaryKey.Name = ParseConstantArgument<string>(context, nameArg, null);
            }

            fuzzyPrimaryKey.IsClustered = ParseConstantArgument<bool>(context, clusteredArg, true);

            innerModel.PrimaryKey = fuzzyPrimaryKey;

            return innerModel;
        }

        static FuzzyTableModel ParseTableIndex(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression, FuzzyDatabaseModel partialDatabase)
        {
            (var innerInvocation, var innerMethodName) = PeekChain(expression);

            var innerModel = (innerMethodName == "PrimaryKey") ? ParseTablePrimaryKey(context, innerInvocation, partialDatabase) :
                             (innerMethodName == "Index") ? ParseTableIndex(context, innerInvocation, partialDatabase) :
                             throw new NotImplementedException();
            
            var arguments = ParseArguments(context, expression);

            var keyColumnsArg = arguments["keyColumns"];
            var uniqueArg = arguments["unique"];
            var nameArg = arguments["name"];
            var clusteredArg = arguments["clustered"];
            var includedColumnsArg = arguments["includedColumns"];

            var fuzzyIndex = new FuzzyIndexModel()
            {
                Columns = ParseOrderedMultiProperty(context, arguments["keyColumns"], "", innerModel.Columns)
            };
            
            if (nameArg == null)
            {
                if (!innerModel.Name.HasValue ||
                    !innerModel.Schema.HasValue ||
                    !fuzzyIndex.Columns.HasValue ||
                    fuzzyIndex.Columns.Value.Any(c => !c.Item2.Name.HasValue))
                {
                    fuzzyIndex.Name = new Optional<string>();
                }
                else
                {
                    fuzzyIndex.Name = new Optional<string>(BuildDefaultKeyName("IX", innerModel.Schema.Value, innerModel.Name.Value, fuzzyIndex.Columns.Value.Select(c => c.Item2.Name.Value)));
                }
            }
            else
            {
                fuzzyIndex.Name = ParseConstantArgument<string>(context, nameArg, null);
            }

            fuzzyIndex.IsUnique = ParseConstantArgument(context, uniqueArg, false);
            fuzzyIndex.IsClustered = ParseConstantArgument(context, clusteredArg, false);

            if (fuzzyIndex.IsClustered.HasValue && fuzzyIndex.IsClustered.Value == true)
            {
                if ((innerModel.PrimaryKey.IsClustered.HasValue && innerModel.PrimaryKey.IsClustered.Value == true) ||
                    (innerModel.Indicies.Any(i => i.IsClustered.HasValue && i.IsClustered.Value == true)))
                    context.ReportDiagnostic(Diagnostic.Create(_descriptors[MultipleClusteredIndicies], clusteredArg.GetLocation()));
            }

            innerModel.Indicies.Add(fuzzyIndex);

            return innerModel;
        }

        static FuzzyTableModel ParseTableBuild(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression, FuzzyDatabaseModel partialDatabase)
        {
            (var innerInvocation, var innerMethodName) = PeekChain(expression);

            if (innerMethodName == "PrimaryKey")
                return ParseTablePrimaryKey(context, innerInvocation, partialDatabase);
            if (innerMethodName == "Index")
                return ParseTableIndex(context, innerInvocation, partialDatabase);
            else
                throw new NotImplementedException();
        }

        static FuzzyDatabaseModel ParseDatabase(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression)
        {
            var nameArgument = expression.ArgumentList.Arguments[0];
            
            return new FuzzyDatabaseModel()
            {
               Name = ParseConstantArgument<string>(context, nameArgument, null),
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
