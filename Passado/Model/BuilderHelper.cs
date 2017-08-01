using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Passado.Model
{
    public static class BuilderHelper
    {
        /// <summary>
        /// Returns either the name of the property or null if the selector is not a simple property access.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static string ParseSelector<TObject, TProperty>(Expression<Func<TObject, TProperty>> selector)
        {
            var memberExpression = selector?.Body as MemberExpression;

            if (memberExpression?.Member == null ||
                memberExpression.Expression.NodeType != ExpressionType.Parameter ||
                typeof(TObject).GetTypeInfo().GetProperty(memberExpression.Member.Name) == null)
            {
                return null;
            }
            else
            {
                return memberExpression.Member.Name;
            }
        }

        public static string GetTableName(string schema, string table)
        {
            if (schema == null)
                return table;
            else
                return $"{schema}.{table}";
        }
    }
}
