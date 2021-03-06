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
using Passado.Error;
using Passado.Model;

using AH = Passado.Analyzers.AnalyzerHelpers;

namespace Passado.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ModelAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ModelBuilderErrorExtensions.AllDiagnostics();

        static string BuildDefaultKeyName(string prefix, string schema, string tableName, IEnumerable<string> columns)
        {
            return $"{prefix}_{(schema != null ? $"{schema}_" : "")}{tableName}__{string.Join("_", columns)}";
        }

        static string BuildDefaultForiegnKeyName(string schema, string tableName, IEnumerable<string> keyColumns, string referenceSchema, string referenceTableName)
        {
            return $"FK_{(schema != null ? $"{schema}_" : "")}{tableName}__{string.Join("_", keyColumns)}__{(referenceSchema != null ? $"{referenceSchema}_" : "")}{referenceTableName}";
        }

        static string ToString(Optional<string> name)
        {
            return name.HasValue ? name.Value : "?";
        }

        static string ToString(Optional<FuzzyProperty> property)
        {
            return property.HasValue ? property.Value.Name : "?";
        }

        static string ToTypeString(Optional<FuzzyProperty> property)
        {
            if (property.HasValue)
                return (property.Value.Type as INamedTypeSymbol).TypeArguments[0].Name;
            else
                return "?";
        }

        static string ToString(FuzzyTableModel table)
        {
            var schema = table.Schema.HasValue ? table.Schema.Value : "?";
            var name = table.Name.HasValue ? table.Name.Value : "?";

            return schema == null ? name : $"{schema}.{name}";
        }

        public static FuzzyTableModel ParseTableChain(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression, FuzzyDatabaseModel partialDatabase)
        {
            (var innerInvocation, var innerMethodName) = AH.PeekChain(expression);

            return innerMethodName == nameof(TableModelBuilderExtensions.Table)      ? ParseTableTable(context, innerInvocation, partialDatabase) :
                   innerMethodName == nameof(TableModelBuilderExtensions.Column)     ? ParseTableColumn(context, innerInvocation, partialDatabase) :
                   innerMethodName == nameof(TableModelBuilderExtensions.PrimaryKey) ? ParseTablePrimaryKey(context, innerInvocation, partialDatabase) :
                   innerMethodName == nameof(TableModelBuilderExtensions.Index)      ? ParseTableIndex(context, innerInvocation, partialDatabase) :
                   innerMethodName == nameof(TableModelBuilderExtensions.ForeignKey) ? ParseTableForeignKey(context, innerInvocation, partialDatabase) :
                   throw new NotImplementedException();
        }

        static FuzzyTableModel ParseTableTable(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression, FuzzyDatabaseModel partialDatabase)
        {
            var arguments = AH.ParseArguments(context, expression);

            var tableArg = arguments["table"];
            var nameArg = arguments["name"];
            var schemaArg = arguments["schema"];

            if (AH.IsNull(context, tableArg))
                context.ReportDiagnostic(BuilderError.ArgumentNull("table").MakeDiagnostic(tableArg.Expression.GetLocation()));

            var property = AH.ParseSelector(context, tableArg);

            var fuzzyTableModel = new FuzzyTableModel()
            {
                Property = property,
                Columns = new List<FuzzyColumnModel>(),
                Indexes = new List<FuzzyIndexModel>(),
                ForeignKeys = new List<FuzzyForeignKeyModel>(),
                Schema = AH.ParseConstantArgument(context, schemaArg, () => AH.Just(null as string)),
                Name = AH.ParseConstantArgument(context, nameArg, () => property.HasValue ?
                                                                        AH.Just(property.Value?.Name) : 
                                                                        new Optional<string>())
            };

            if (fuzzyTableModel.Property.HasValue)
            {
                var priorTable = partialDatabase.Tables.FirstOrDefault(t => t.Property.HasValue && t.Property.Value.Name == fuzzyTableModel.Property.Value.Name);
                if (priorTable != null)
                    context.ReportDiagnostic(ModelBuilderError.TableRepeatedSelector(ToString(partialDatabase.Name), ToString(fuzzyTableModel.Property), ToString(priorTable)).MakeDiagnostic(tableArg.GetLocation()));
            }
            
            if (fuzzyTableModel.Name.HasValue && fuzzyTableModel.Schema.HasValue)
            {
                var priorTable = partialDatabase.Tables.FirstOrDefault(t => t.Name.HasValue &&
                                                                            t.Schema.HasValue &&
                                                                            t.Name.Value == fuzzyTableModel.Name.Value &&
                                                                            t.Schema.Value == fuzzyTableModel.Schema.Value);
                if (priorTable != null)
                    context.ReportDiagnostic(ModelBuilderError.TableRepeatedName(ToString(priorTable)).MakeDiagnostic(nameArg != null ? nameArg.GetLocation() : tableArg.GetLocation(), schemaArg != null ? new List<Location>() { schemaArg.GetLocation() } : null));
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
            var innerModel = ParseTableChain(context, expression, partialDatabase);

            var arguments = AH.ParseArguments(context, expression);
            var columnArg = arguments["column"];
            var typeArg = arguments["type"];
            var nullableArg = arguments["nullable"];
            var maxLengthArg = arguments["maxLength"];
            var nameArg = arguments["name"];
            var defaultValueArg = arguments["defaultValue"];
            var identityArg = arguments["identity"];
            var converterArg = arguments["converter"];

            if (AH.IsNull(context, columnArg))
                context.ReportDiagnostic(BuilderError.ArgumentNull("column").MakeDiagnostic(columnArg.GetLocation()));
            
            var property = AH.ParseSelector(context, columnArg);

            var fuzzyColumnModel = new FuzzyColumnModel()
            {
                Property = property,
                MaxLength = AH.ParseConstantArgument(context, maxLengthArg, () => AH.Just(null as int?)),
                IsIdentity = AH.ParseConstantArgument(context, identityArg, () => AH.Just(false)),
                IsNullable = AH.ParseConstantArgument(context, nullableArg, () => AH.Just(false)),
                Type = AH.ParseConstantArgument(context, typeArg, () => new Optional<SqlType>()),
                Name = AH.ParseConstantArgument(context, nameArg, () => !property.HasValue ?
                                                                        new Optional<string>() :
                                                                        AH.Just(property.Value?.Name))
            };
        


            if (fuzzyColumnModel.Property.HasValue)
            {
                var otherColumn = innerModel.Columns.FirstOrDefault(t => t.Property.HasValue && t.Property.Value.Name == fuzzyColumnModel.Property.Value.Name);
                if (otherColumn != null)
                    context.ReportDiagnostic(ModelBuilderError.ColumnRepeatedSelector(ToTypeString(innerModel.Property), ToString(fuzzyColumnModel.Property), ToString(otherColumn.Name)).MakeDiagnostic(columnArg.GetLocation()));
            }

            if (fuzzyColumnModel.Name.HasValue)
            {
                if (innerModel.Columns.Any(c => c.Name.HasValue &&
                                                c.Name.Value == fuzzyColumnModel.Name.Value))
                    context.ReportDiagnostic(ModelBuilderError.ColumnRepeatedName(ToString(innerModel.Name), ToString(fuzzyColumnModel.Name)).MakeDiagnostic(nameArg == null ? columnArg.GetLocation() : nameArg.GetLocation()));
            }
            
            if (fuzzyColumnModel.Property.HasValue && fuzzyColumnModel.Type.HasValue)
            {
                if (fuzzyColumnModel.Property.Value.Type.TypeKind == TypeKind.Enum)
                {
                    if (!(fuzzyColumnModel.Type.Value == SqlType.String || SqlTypeHelpers.IsIntegral(fuzzyColumnModel.Type.Value)))
                        context.ReportDiagnostic(ModelBuilderError.ColumnEnumNotStringOrIntegralType().MakeDiagnostic(typeArg.GetLocation()));

                    if (fuzzyColumnModel.Type.Value == SqlType.String && fuzzyColumnModel.MaxLength.HasValue)
                    {
                        var namedType = fuzzyColumnModel.Property.Value.Type as INamedTypeSymbol;
                        var longestEnum = namedType.MemberNames.OrderByDescending(n => n.Length).First();

                        if (fuzzyColumnModel.MaxLength.Value != null &&
                            longestEnum.Length > fuzzyColumnModel.MaxLength.Value)
                            context.ReportDiagnostic(ModelBuilderError.ColumnEnumLongerThanMaxStringSize($"{namedType.Name}.{longestEnum}", (int)fuzzyColumnModel.MaxLength.Value).MakeDiagnostic(maxLengthArg.GetLocation()));
                    }
                }
                else if (fuzzyColumnModel.Property.Value.Type.Name == "DateTimeOffset" ||
                         fuzzyColumnModel.Property.Value.Type.Name == "Guid")
                {
                    //if ((fuzzyColumnModel.Property.Value.Type.Name == "DateTimeOffset" && fuzzyColumnModel.Type.Value != SqlType.DateTimeOffset) ||
                    //    (fuzzyColumnModel.Property.Value.Type.Name == "Guid" && fuzzyColumnModel.Type.Value != SqlType.Guid))
                    //    context.ReportDiagnostic(Diagnostic.Create(_descriptors[InvalidSqlType], typeArg.GetLocation()));
                }
                else
                {
                    var propertyType = fuzzyColumnModel.Property.Value.Type.SpecialType;
                    
                    //if (!_defaultTypeMappings[propertyType].Contains(fuzzyColumnModel.Type.Value))
                    //    context.ReportDiagnostic(Diagnostic.Create(_descriptors[InvalidSqlType], typeArg.GetLocation()));
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

                //if (nonIdentityTypes.Contains(fuzzyColumnModel.Type.Value))
                //    context.ReportDiagnostic(Diagnostic.Create(_descriptors[InvalidSqlTypeForIdentity], identityArg.GetLocation()));
            }

            if (fuzzyColumnModel.IsIdentity.HasValue && fuzzyColumnModel.IsIdentity.Value == true &&
                fuzzyColumnModel.IsNullable.HasValue && fuzzyColumnModel.IsNullable.Value == true)
                context.ReportDiagnostic(ModelBuilderError.ColumnIdentityNullable().MakeDiagnostic(nullableArg.GetLocation()));

            innerModel.Columns.Add(fuzzyColumnModel);
            
            return innerModel;
        }

        static FuzzyTableModel ParseTablePrimaryKey(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression, FuzzyDatabaseModel partialDatabase)
        {
            var innerModel = ParseTableChain(context, expression, partialDatabase);

            var arguments = AH.ParseArguments(context, expression);

            var keyColumnsArg = arguments["keyColumns"];
            var nameArg = arguments["name"];
            var clusteredArg = arguments["clustered"];

            var columns = new Optional<ImmutableArray<(SortOrder, FuzzyColumnModel)>>();

            if (AH.IsNull(context, keyColumnsArg))
                context.ReportDiagnostic(BuilderError.ArgumentNull("keyColumns").MakeDiagnostic(keyColumnsArg.GetLocation()));
            else
            {
                var props = AH.ParseOrderedMultiProperty(context, keyColumnsArg);
                if (props.HasValue)
                {
                    columns = AH.MatchColumns(context, props.Value, innerModel.Columns, ToString(innerModel.Name));
                }
            }
            
            var fuzzyPrimaryKey = new FuzzyPrimaryKeyModel()
            {
                Columns = columns,
                IsClustered = AH.ParseConstantArgument(context, clusteredArg, () => AH.Just(true)),
                Name = AH.ParseConstantArgument(context, nameArg, () => (!innerModel.Name.HasValue || !innerModel.Schema.HasValue || !columns.HasValue || columns.Value.Any(c => !c.Item2.Name.HasValue)) ?
                                                                        new Optional<string>() : 
                                                                        AH.Just(BuildDefaultKeyName("PK", innerModel.Schema.Value, innerModel.Name.Value, columns.Value.Select(c => c.Item2.Name.Value))))
            };

            innerModel.PrimaryKey = fuzzyPrimaryKey;

            return innerModel;
        }
        
        static FuzzyTableModel ParseTableIndex(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression, FuzzyDatabaseModel partialDatabase)
        {
            var innerModel = ParseTableChain(context, expression, partialDatabase);
            
            var arguments = AH.ParseArguments(context, expression);

            var keyColumnsArg = arguments["keyColumns"];
            var uniqueArg = arguments["unique"];
            var nameArg = arguments["name"];
            var clusteredArg = arguments["clustered"];
            var includedColumnsArg = arguments["includedColumns"];

            var keyColumns = new Optional<ImmutableArray<(SortOrder, FuzzyColumnModel)>>();

            if (AH.IsNull(context, keyColumnsArg))
                context.ReportDiagnostic(BuilderError.ArgumentNull("keyColumns").MakeDiagnostic(keyColumnsArg.GetLocation()));
            else
            {
                var props = AH.ParseOrderedMultiProperty(context, keyColumnsArg);
                if (props.HasValue)
                {
                    keyColumns = AH.MatchColumns(context, props.Value, innerModel.Columns, ToString(innerModel.Name));
                }
            }

            var includedColumns = new Optional<ImmutableArray<FuzzyColumnModel>>();

            if (!AH.IsNull(context, includedColumnsArg))
            {
                var props = AH.ParseMultiProperty(context, includedColumnsArg);
                if (props.HasValue)
                {
                    var match = AH.MatchColumns(context, props.Value, innerModel.Columns, ToString(innerModel.Name));
                    includedColumns = match.HasValue ? AH.Just(match.Value.Select(m => m.Item1).ToImmutableArray()) : new Optional<ImmutableArray<FuzzyColumnModel>>();

                    if (keyColumns.HasValue)
                    {
                        var column = props.Value.Select(v => ((FuzzyProperty, Location)?)v)
                                                .FirstOrDefault(i => keyColumns.Value.Any(k => k.Item2.Property.HasValue && k.Item2.Property.Value.Name == i.Value.Item1.Name));
                        if (column != null)
                            context.ReportDiagnostic(ModelBuilderError.IndexIncludedColumnAlreadyInKeyColumns(column.Value.Item1.Name).MakeDiagnostic(column.Value.Item2));
                    }
                }
            }
            
            var fuzzyIndex = new FuzzyIndexModel()
            {
                KeyColumns = keyColumns,
                IsUnique = AH.ParseConstantArgument(context, uniqueArg, () => AH.Just(false)),
                IsClustered = AH.ParseConstantArgument(context, clusteredArg, () => AH.Just(false)),
                IncludedColumns = includedColumns,
                Name = AH.ParseConstantArgument(context, nameArg, () => (!innerModel.Name.HasValue || !innerModel.Schema.HasValue || !keyColumns.HasValue || keyColumns.Value.Any(c => !c.Item2.Name.HasValue)) ?
                                                                        new Optional<string>() :
                                                                        AH.Just(BuildDefaultKeyName("IX", innerModel.Schema.Value, innerModel.Name.Value, keyColumns.Value.Select(c => c.Item2.Name.Value))))
            };
            
            if (fuzzyIndex.IsClustered.HasValue && fuzzyIndex.IsClustered.Value == true)
            {
                var priorClustered = (innerModel.PrimaryKey?.IsClustered)?.HasValue == true && innerModel.PrimaryKey.IsClustered.Value ?
                                        innerModel.PrimaryKey.Name.Value : 
                                        innerModel.Indexes.FirstOrDefault(i => i.IsClustered.HasValue && i.IsClustered.Value)?.Name.Value;
                if (priorClustered != null)
                    context.ReportDiagnostic(ModelBuilderError.IndexClusteredAlreadySpecified(priorClustered).MakeDiagnostic(clusteredArg.Expression.GetLocation()));
            }

            innerModel.Indexes.Add(fuzzyIndex);

            return innerModel;
        }

        static FuzzyTableModel ParseTableForeignKey(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression, FuzzyDatabaseModel partialDatabase)
        {
            var innerModel = ParseTableChain(context, expression, partialDatabase);

            var arguments = AH.ParseArguments(context, expression);

            var keyColumnsArg = arguments["keyColumns"];
            var referenceTableArg = arguments["referenceTable"];
            var referenceColumnsArg = arguments["referenceColumns"];
            var updateActionArg = arguments["updateAction"];
            var deleteActionArg = arguments["deleteAction"];
            var nameArg = arguments["name"];

            var fuzzyForeignKey = new FuzzyForeignKeyModel();

            if (AH.IsNull(context, keyColumnsArg))
            {
                context.ReportDiagnostic(BuilderError.ArgumentNull("keyColumns").MakeDiagnostic(keyColumnsArg.Expression.GetLocation()));
                fuzzyForeignKey.KeyColumns = new Optional<ImmutableArray<(FuzzyColumnModel, Location)>>();
            }
            else
            {
                var keyColumns = AH.ParseMultiProperty(context, keyColumnsArg);

                if (keyColumns.HasValue)
                {
                    fuzzyForeignKey.KeyColumns = AH.MatchColumns(context, keyColumns.Value, innerModel.Columns, ToString(innerModel.Name));
                }
            }

            if (AH.IsNull(context, referenceTableArg))
            {
                context.ReportDiagnostic(BuilderError.ArgumentNull("referenceTable").MakeDiagnostic(referenceTableArg.Expression.GetLocation()));
                fuzzyForeignKey.ReferenceTableSelector = new Optional<(FuzzyProperty, Location)>();
            }
            else
            {
                var selector = AH.ParseSelector(context, referenceTableArg);
                fuzzyForeignKey.ReferenceTableSelector = selector.HasValue ? AH.Just((selector.Value, (referenceTableArg.Expression as LambdaExpressionSyntax).Body.GetLocation())) :
                                                                             new Optional<(FuzzyProperty, Location)>();
            }

            if (AH.IsNull(context, referenceColumnsArg))
            {
                context.ReportDiagnostic(BuilderError.ArgumentNull("referenceColumns").MakeDiagnostic(referenceColumnsArg.Expression.GetLocation()));
                fuzzyForeignKey.ReferenceColumnSelectors = new Optional<ImmutableArray<(FuzzyProperty, Location)>>();
            }
            else
            {
                fuzzyForeignKey.ReferenceColumnSelectors = AH.ParseMultiProperty(context, referenceColumnsArg);
            }

            if (fuzzyForeignKey.KeyColumns.HasValue &&
                fuzzyForeignKey.ReferenceColumnSelectors.HasValue &&
                fuzzyForeignKey.KeyColumns.Value.Length != fuzzyForeignKey.ReferenceColumnSelectors.Value.Length)
            {
                context.ReportDiagnostic(ModelBuilderError.ForeignKeyColumnCountsDontMatch().MakeDiagnostic(AH.DeconstructLambda(keyColumnsArg).Body.GetLocation(), new List<Location>() { AH.DeconstructLambda(referenceColumnsArg).Body.GetLocation() } ));
            }

            fuzzyForeignKey.UpdateAction = AH.ParseConstantArgument(context, updateActionArg, () => AH.Just(ForeignKeyAction.Cascade));
            fuzzyForeignKey.DeleteAction = AH.ParseConstantArgument(context, deleteActionArg, () => AH.Just(ForeignKeyAction.Cascade));

            innerModel.ForeignKeys.Add(fuzzyForeignKey);

            return innerModel;
        }
        
        static FuzzyTableModel ParseTableBuild(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression, FuzzyDatabaseModel partialDatabase)
        {
            var innerModel = ParseTableChain(context, expression, partialDatabase);

            return innerModel;
        }

        public static FuzzyDatabaseModel ParseDatabaseChain(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression)
        {
            (var innerInvocation, var innerMethodName) = AH.PeekChain(expression);

            return innerMethodName == nameof(DatabaseModelBuilderExtensions.Database) ? ParseDatabase(context, innerInvocation) :
                   innerMethodName == nameof(DatabaseModelBuilderExtensions.Table) ? ParseTable(context, innerInvocation) :
                   throw new NotImplementedException();
        }

        static FuzzyDatabaseModel ParseDatabase(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression)
        {
            var arguments = AH.ParseArguments(context, expression);
            var nameArg = arguments["name"];

            if (AH.IsNull(context, nameArg))
                context.ReportDiagnostic(BuilderError.ArgumentNull("name").MakeDiagnostic(nameArg.GetLocation()));

            var databaseModel = new FuzzyDatabaseModel()
            {
               Name = AH.ParseConstantArgument<string>(context, nameArg, null),
               Tables = new List<FuzzyTableModel>()
            };
            
            return databaseModel;
        }

        static FuzzyDatabaseModel ParseTable(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression)
        {
            var innerModel = ParseDatabaseChain(context, expression);
            
            var arguments = AH.ParseArguments(context, expression);

            var tableArg = arguments["table"];
            
            if (AH.IsNull(context, tableArg))
            {
                context.ReportDiagnostic(BuilderError.ArgumentNull("table").MakeDiagnostic(tableArg.GetLocation()));
            }
            else if (tableArg.Expression is MemberAccessExpressionSyntax)
            {
                var _ = context.SemanticModel.GetSymbolInfo(tableArg.Expression);
                
                // Static class function reference (e.g. User.ProvideModel)
                var staticFunctionMember = tableArg.Expression as MemberAccessExpressionSyntax;
                var className = (staticFunctionMember.Expression as IdentifierNameSyntax).Identifier.Text;
                var functionName = (staticFunctionMember?.Name?.Identifier)?.Text;

                throw new NotImplementedException();
            }
            else if (tableArg.Expression is SimpleLambdaExpressionSyntax)
            {
                // Defined inline
                //ParseTableChain(context, firstArgument.Expression);
                var lambdaExpression = tableArg.Expression as SimpleLambdaExpressionSyntax;

                var table = ParseTableBuild(context, lambdaExpression.Body as InvocationExpressionSyntax, innerModel);

                innerModel.Tables.Add(table);
            }

            return innerModel;
        }

        static void ParseDatabaseBuild(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax expression)
        {
            var innerModel = ParseDatabaseChain(context, expression);

            foreach (var table in innerModel.Tables)
            {
                foreach (var foreignKey in table.ForeignKeys)
                {
                    if (foreignKey.ReferenceTableSelector.HasValue)
                    {
                        var referenceTable = innerModel.Tables.FirstOrDefault(t => t.Property.HasValue && t.Property.Value.Name == foreignKey.ReferenceTableSelector.Value.Item1.Name);

                        if (referenceTable == null)
                        {
                            context.ReportDiagnostic(BuilderError.SelectorNotMappedToTable(foreignKey.ReferenceTableSelector.Value.Item1.Name, ToString(innerModel.Name)).MakeDiagnostic(foreignKey.ReferenceTableSelector.Value.Item2));
                            foreignKey.ReferenceTable = new Optional<FuzzyTableModel>();
                            break;
                        }
                        else
                        {
                            foreignKey.ReferenceTable = new Optional<FuzzyTableModel>(referenceTable);
                        }
                        
                        foreignKey.ReferenceColumns = AH.MatchColumns(context, foreignKey.ReferenceColumnSelectors.Value, referenceTable.Columns, ToString(referenceTable.Name));

                        if (!foreignKey.Name.HasValue &&
                            table.Schema.HasValue &&
                            table.Name.HasValue &&
                            foreignKey.KeyColumns.HasValue &&
                            foreignKey.KeyColumns.Value.All(k => k.Model.Name.HasValue) &&
                            foreignKey.ReferenceTable.HasValue &&
                            foreignKey.ReferenceTable.Value.Schema.HasValue &&
                            foreignKey.ReferenceTable.Value.Name.HasValue)
                        {
                            foreignKey.Name = BuilderHelper.GenerateForeignKeyName(table.Schema.Value,
                                                                                   table.Name.Value,
                                                                                   foreignKey.KeyColumns.Value.Select(k => k.Model.Name.Value),
                                                                                   foreignKey.ReferenceTable.Value.Schema.Value,
                                                                                   foreignKey.ReferenceTable.Value.Name.Value);
                        }
                        
                        if (foreignKey.KeyColumns.HasValue)
                        {
                            foreach (var columnPair in foreignKey.KeyColumns.Value.Zip(foreignKey.ReferenceColumns.Value, (l, r) => (l, r)))
                            {
                                if (columnPair.Item1.Model.Type.HasValue && columnPair.Item2.Model.Type.HasValue && columnPair.Item1.Model.Type.Value != columnPair.Item2.Model.Type.Value)
                                {
                                    context.ReportDiagnostic(ModelBuilderError.ForeignKeyColumnTypesDontMatch(foreignKeyName: ToString(foreignKey.Name),
                                                                                                              keyColumnName: ToString(columnPair.Item1.Model.Name),
                                                                                                              keyColumnType: columnPair.Item1.Model.Type.Value.ToString(),
                                                                                                              referenceColumnName: ToString(columnPair.Item2.Model.Name),
                                                                                                              referenceColumnType: columnPair.Item2.Model.Type.Value.ToString()).MakeDiagnostic(columnPair.Item1.Location, new List<Location>() { columnPair.Item2.Location }));
                                }
                            }
                        }
                    }
                }
            }  
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

                    if (methodSymbol?.ToString()?.StartsWith("Passado.Model.Database.IDatabaseModelBuilder") == true)
                    {
                        ParseDatabaseBuild(syntaxContext, invocationExpression);
                    }
                }

            }, SyntaxKind.InvocationExpression);
        }
    }
}
