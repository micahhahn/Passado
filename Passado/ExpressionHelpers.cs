using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Passado.Model;
using Passado.Model.Table;

namespace Passado
{
    public static class ExpressionHelpers
    {
        public static PropertyInfo ParsePropertySelector(LambdaExpression expression)
        {
            if (expression.Body is MemberExpression memberExpression &&
                memberExpression.Expression.NodeType == ExpressionType.Parameter)
            {
                return memberExpression.Member as PropertyInfo;
            }
            else
            {
                return null;
            }
        }

        public static List<PropertyInfo> ParseMultiPropertySelector(LambdaExpression properties)
        {
            PropertyInfo ParsePropertySelector(Expression expression)
            {
                if (expression is MemberExpression propertyMemberExpression)
                {
                    return propertyMemberExpression.Member as PropertyInfo;
                }

                throw new NotImplementedException();
            }

            if (properties.Body is UnaryExpression convertExpression && convertExpression.NodeType == ExpressionType.Convert)
            {
                return new List<PropertyInfo>()
                {
                    ParsePropertySelector(convertExpression.Operand)
                };
            }
            else if (properties.Body is NewExpression newExpression)
            {
                return newExpression.Arguments
                                    .Select(a => ParsePropertySelector(a))
                                    .ToList();
            }

            throw new NotImplementedException();
        }

        public static List<ColumnModel> MatchColumns(this IEnumerable<PropertyInfo> properties, IEnumerable<ColumnModel> columns)
        {
            return properties.Select(p =>
            {
                var column = columns.FirstOrDefault(c => c.Property.Name == p.Name);

                if (column == null)
                    throw new NotImplementedException();

                return column;
            }).ToList();
        }

        /// <summary>
        /// Pulls properties out of a 
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>t => t.Asc.Prop1</para>
        ///     <para>t => new { t.Asc.Prop1 }</para>
        ///     <para>t => new { t.Asc.Prop1, t.Desc.Prop2 }</para>
        /// </remarks>
        public static List<(PropertyInfo Property, SortOrder Order)> ParseOrderedMultiPropertySelector(LambdaExpression properties)
        {
            (PropertyInfo, SortOrder) ParsePropertySelector(Expression expression)
            {
                if (expression is MemberExpression propertyMemberExpression)
                {
                    if (propertyMemberExpression.Expression is MemberExpression orderMemberExpression)
                    {
                        var sortOrder = orderMemberExpression.Member.Name == "Asc" ? SortOrder.Ascending :
                                        orderMemberExpression.Member.Name == "Desc" ? SortOrder.Descending :
                                        throw new NotImplementedException();

                        return (propertyMemberExpression.Member as PropertyInfo, sortOrder);
                    }
                }

                throw ModelBuilderError.OrderedMultiColumnSelectorMemberAccessNotOnParameter().AsException();
            }

            if (properties.Body is UnaryExpression convertExpression && convertExpression.NodeType == ExpressionType.Convert)
            {
                return new List<(PropertyInfo Property, SortOrder Order)>()
                {
                    ParsePropertySelector(convertExpression.Operand)
                };
            }
            else if (properties.Body is NewExpression newExpression)
            {
                return newExpression.Arguments
                                    .Select(a => ParsePropertySelector(a))
                                    .ToList();
            }
            else
            {
                throw ModelBuilderError.OrderedMultiColumnSelectorNotMemberOrAnonymousObject().AsException();
            }
        }

        public static List<SortedColumnModel> MatchColumns(this IEnumerable<(PropertyInfo Property, SortOrder Order)> orderedProperties, IEnumerable<ColumnModel> columns)
        {
            return orderedProperties.Select(p =>
            {
                var column = columns.FirstOrDefault(c => c.Property.Name == p.Property.Name);

                if (column == null)
                    throw new NotImplementedException();

                return new SortedColumnModel(column, p.Order);
            }).ToList();
        }
    }
}
