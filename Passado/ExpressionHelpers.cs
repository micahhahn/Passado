using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

using Passado.Error;
using Passado.Model;
using Passado.Model.Table;

namespace Passado
{
    public static class ExpressionHelpers
    {
        public static PropertyInfo ParseSelector(LambdaExpression expression)
        {
            if (expression.Body is MemberExpression memberExpression &&
                memberExpression.Member is PropertyInfo property &&
                memberExpression.Expression.NodeType == ExpressionType.Parameter)
                return property;

            throw BuilderError.SelectorInvalid(expression.Parameters[0].Name).AsException();
        }

        public static PropertyModel ParsePropertySelector(LambdaExpression expression)
        {
            var selector = ParseSelector(expression);
            return new PropertyModel(selector.Name, selector.PropertyType);
        }

        static ImmutableArray<TSelector> ParseMultiSelector<TSelector>(LambdaExpression properties, Func<Expression, TSelector> parseSelector)
        {
            // Types of selectors:
            // 1. t.Prop1
            // 2. t.Asc.Prop1
            // 3. t.Table1.Prop1

            if (properties.Body is MemberExpression)
            {
                return ImmutableArray.Create(parseSelector(properties.Body));
            }
            else if (properties.Body is UnaryExpression convertExpression && convertExpression.NodeType == ExpressionType.Convert)
            {
                // Struct types will undergo an implicit boxing conversion so the top level expression will be a conversion
                // TODO: Do we need to check the type...? can an arbitrary (and undesirable) case slip in here?
                return ImmutableArray.Create(parseSelector(convertExpression.Operand));
            }
            else if (properties.Body is NewExpression newExpression)
            {
                return newExpression.Arguments
                                    .Select(a => parseSelector(a))
                                    .ToImmutableArray();
            }

            throw new NotImplementedException();
        }

        public static ImmutableArray<(PropertyInfo InnerProperty, PropertyInfo OutProperty)> ParseNestedMultiPropertySelector(LambdaExpression properties)
        {
            return ParseMultiSelector(properties, expression =>
            {
                if (expression is MemberExpression outerMemberExpression &&
                    outerMemberExpression.Member is PropertyInfo outerProperty &&
                    outerMemberExpression.Expression is MemberExpression innerMemberExpression &&
                    innerMemberExpression.Member is PropertyInfo innerProperty &&
                    innerMemberExpression.Expression.NodeType == ExpressionType.Parameter)
                    return (innerProperty, outerProperty);

                throw new NotImplementedException();
            });
        }

        //public static ImmutableArray<(PropertyInfo Property, SortOrder Order)> ParseOrderedMultiPropertySelector(LambdaExpression properties)
        //{
        //    return ParseMultiSelector(properties, expression =>
        //    {

        //    });
        //}

        public static ImmutableArray<PropertyInfo> ParseMultiPropertySelector(LambdaExpression properties)
        {
            PropertyInfo ParsePropertySelector(Expression expression)
            {
                if (expression is MemberExpression propertyMemberExpression &&
                    propertyMemberExpression.Member is PropertyInfo property &&
                    propertyMemberExpression.Expression.NodeType == ExpressionType.Parameter)
                    return property;

                throw BuilderError.SelectorInvalid(properties.Parameters[0].Name).AsException();
            }

            if (properties.Body is MemberExpression)
            {
                return ImmutableArray.Create(ParsePropertySelector(properties.Body));
            }
            else if (properties.Body is UnaryExpression convertExpression && convertExpression.NodeType == ExpressionType.Convert)
            {
                // Struct types will undergo an implicit boxing conversion so the top level expression will be a conversion
                return ImmutableArray.Create(ParsePropertySelector(convertExpression.Operand));
            }
            else if (properties.Body is NewExpression newExpression)
            {
                return newExpression.Arguments
                                    .Select(a => ParsePropertySelector(a))
                                    .ToImmutableArray();
            }

            throw BuilderError.MultiSelectorInvalid(properties.Parameters[0].Name).AsException();
        }

        public static ImmutableArray<ColumnModel> MatchColumns(this IEnumerable<PropertyInfo> properties, string tableName, IEnumerable<ColumnModel> columns)
        {
            return properties.Select(p =>
            {
                var column = columns.FirstOrDefault(c => c.Property.Name == p.Name);

                if (column == null)
                    throw BuilderError.SelectorNotMappedToColumn(p.Name, tableName).AsException();

                return column;
            }).ToImmutableArray();
        }

        public static ImmutableArray<(PropertyInfo Property, SortOrder Order)> ParseOrderedMultiPropertySelector(LambdaExpression properties)
        {
            (PropertyInfo, SortOrder) ParsePropertySelector(Expression expression)
            {
                if (expression is MemberExpression propertyMemberExpression)
                {
                    if (propertyMemberExpression.Expression is MemberExpression orderMemberExpression &&
                        propertyMemberExpression.Member is PropertyInfo property &&
                        orderMemberExpression.Expression.NodeType == ExpressionType.Parameter)
                    {
                        var sortOrder = orderMemberExpression.Member.Name == "Asc" ? SortOrder.Ascending :
                                        orderMemberExpression.Member.Name == "Desc" ? SortOrder.Descending :
                                        throw new NotImplementedException();

                        return (property, sortOrder);
                    }
                }
                
                throw BuilderError.OrderedSelectorInvalid(parameterName: properties.Parameters[0].Name).AsException();
            }
            
            if (properties.Body is MemberExpression)
            {
                return ImmutableArray.Create(ParsePropertySelector(properties.Body));
            }
            else if (properties.Body is UnaryExpression convertExpression && convertExpression.NodeType == ExpressionType.Convert)
            {
                // Struct types will undergo an implicit boxing conversion so the top level expression will be a conversion
                return ImmutableArray.Create(ParsePropertySelector(convertExpression.Operand));
            }
            else if (properties.Body is NewExpression newExpression)
            {
                return newExpression.Arguments
                                    .Select(a => ParsePropertySelector(a))
                                    .ToImmutableArray();
            }
            
            throw BuilderError.OrderedMultiSelectorInvalid(parameterName: properties.Parameters[0].Name).AsException();
        }

        public static ImmutableArray<SortedColumnModel> MatchColumns(this IEnumerable<(PropertyInfo Property, SortOrder Order)> orderedProperties, string tableName, IEnumerable<ColumnModel> columns)
        {
            return orderedProperties.Select(p =>
            {
                var column = columns.FirstOrDefault(c => c.Property.Name == p.Property.Name);

                if (column == null)
                    throw BuilderError.SelectorNotMappedToColumn(p.Property.Name, tableName).AsException();
                
                return new SortedColumnModel(column, p.Order);
            }).ToImmutableArray();
        }
    }
}
