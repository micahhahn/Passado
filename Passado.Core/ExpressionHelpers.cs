using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Passado.Core.Model;

namespace Passado.Core
{
    public static class ExpressionHelpers
    {
        /// <summary>
        /// Pulls properties out of a 
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>t => t.Prop1</para>
        ///     <para>t => (Desc)t.Prop1</para>
        ///     <para>t => new { t.Prop1, t.Prop2 }</para>
        ///     <para>t => new { C1 = (Asc)t.Prop1, C2 = (Desc)t.Prop2 }</para>
        /// </remarks>
        public static List<(PropertyInfo Property, SortOrder? Order)> ParseMultiPropertySelector(LambdaExpression columns)
        {
            (PropertyInfo, SortOrder?)? ParsePropertySelector(Expression expression)
            {
                var sortOrder = null as SortOrder?;

                if (expression.NodeType == ExpressionType.Convert)
                {
                    var unaryExpression = expression as UnaryExpression;

                    if (unaryExpression.Type != typeof(Asc) && unaryExpression.Type != typeof(Desc))
                        return null;

                    sortOrder = unaryExpression.Type == typeof(Asc) ? SortOrder.Ascending : SortOrder.Descending;
                    expression = unaryExpression.Operand;
                }

                if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    var memberExpression = expression as MemberExpression;

                    if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                    {
                        return (memberExpression.Member as PropertyInfo, sortOrder);
                    }
                }

                return null;
            }

            if (columns.Body.NodeType == ExpressionType.Convert)
            {
                var unaryExpression = columns.Body as UnaryExpression;

                if (unaryExpression.Type == typeof(object) ||
                    unaryExpression.Type == typeof(Asc) ||
                    unaryExpression.Type == typeof(Desc))
                {
                    var property = ParsePropertySelector(unaryExpression.Type == typeof(object) ? unaryExpression.Operand : unaryExpression);

                    return property == null ? null : new List<(PropertyInfo, SortOrder?)>() { property.Value };
                }
            }
            else if (columns.Body.NodeType == ExpressionType.New)
            {
                var newExpression = columns.Body as NewExpression;

                var ret = new List<(PropertyInfo, SortOrder?)>();

                foreach (var arg in newExpression.Arguments)
                {
                    var property = ParsePropertySelector(arg);

                    if (property == null)
                        return null;

                    ret.Add(property.Value);
                }

                return ret;
            }

            return null;
        }
    }
}
