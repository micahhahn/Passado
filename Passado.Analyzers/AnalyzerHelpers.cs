using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;


namespace Passado.Analyzers
{
    public class AnalyzerHelpers
    {
        public static Type GetSymbolType(SyntaxNodeAnalysisContext context, ITypeSymbol typeSymbol)
        {
            var symbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
            var qualifiedName = typeSymbol.ToDisplayString(symbolDisplayFormat);

            var metaName = typeSymbol.MetadataName;
            

            throw new NotImplementedException();
        }

        public static Expression CompileExpression(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
        {
            if (!(expression is SimpleLambdaExpressionSyntax || expression is ParenthesizedExpressionSyntax))
                throw new NotImplementedException();

            var lambdaInfo = context.SemanticModel.GetSymbolInfo(expression).Symbol as IMethodSymbol;

            //var parameters = lambdaInfo.Parameters
            //                           .Select(p => GetSymbolType(p.Type))
            //                           .ToList();

            //context.SemanticModel.GetTypeInfo(lambdaInfo.Parameters[0]);

            if (expression is SimpleLambdaExpressionSyntax)
            {
                var _ = context.SemanticModel.GetSymbolInfo(expression);

                var simpleLambda = expression as SimpleLambdaExpressionSyntax;
                var parameterType = context.SemanticModel.GetTypeInfo(simpleLambda.Parameter);

                throw new NotImplementedException();
            }
            else if (expression is ParenthesizedExpressionSyntax)
            {

            }
            else
            {
                throw new NotImplementedException();
            }
            
            throw new NotImplementedException();
        }

        // TODO: This should be removed
        public static bool IsSimpleSelector(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
        {
            var lambdaExpression = expression as SimpleLambdaExpressionSyntax;

            if (lambdaExpression == null)
                return false;

            var selector = lambdaExpression.Body as MemberAccessExpressionSyntax;

            if (selector == null)
                return false;

            return context.SemanticModel.GetSymbolInfo(selector.Expression).Symbol is IParameterSymbol;
        }
    }
}
