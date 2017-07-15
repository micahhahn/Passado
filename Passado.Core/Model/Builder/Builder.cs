using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Passado.Core.Model.Builder
{
    public static class Builder
    {
        public static string GetDatabasePrefix<TDatabase>()
        {
            return $"In database '{typeof(TDatabase).Name}':\n";
        }

        public static string GetTablePrefix<TTable>()
        {
            return $"In table '{typeof(TTable).Name}':\n";
        }

        public static string ParsePropertySelector<TClass, TProperty>(Expression<Func<TClass, TProperty>> selector)
        {
            var memberExpression = selector?.Body as MemberExpression;

            if (memberExpression?.Member == null ||
                memberExpression.Expression.NodeType != ExpressionType.Parameter ||
                typeof(TClass).GetTypeInfo().GetProperty(memberExpression.Member.Name) == null)
            {
                return null;
            }
            else
            {
                return memberExpression.Member.Name;
            }
        }

        public static string ParseWeakPropertySelector(LambdaExpression selector)
        {
            var memberExpression = selector?.Body as MemberExpression;

            if (memberExpression?.Member == null ||
                memberExpression.Expression.NodeType != ExpressionType.Parameter ||
                memberExpression.Expression.Type.GetTypeInfo().GetProperty(memberExpression.Member.Name) == null)
            {
                return null;
            }
            else
            {
                return memberExpression.Member.Name;
            }
        }

        static IEnumerable<(ColumnModel Column, SortOrder? Order)> InternalParseColumnSelector(LambdaExpression columns, IEnumerable<ColumnModel> tableColumns)
        {
            var tableType = columns.Parameters[0].Type;

            var properties = ExpressionHelpers.ParseMultiPropertySelector(columns);

            if (properties == null)
                throw new ModelException($"must refer to a property or properties of '{tableType.Name}'.");

            foreach (var property in properties)
            {
                var column = tableColumns.SingleOrDefault(c => c.PropertyName == property.Property.Name);

                if (column == null)
                    throw new ModelException($"can only reference columns of '{tableType.Name}'.");

                yield return (column, property.Order);
            }
        }

        internal static IEnumerable<ColumnModel> ParseColumnSelector(LambdaExpression columnsExpression, IEnumerable<ColumnModel> tableColumns)
        {
            var columns = InternalParseColumnSelector(columnsExpression, tableColumns).ToList();

            if (columns.Any(c => c.Order != null))
                throw new ModelException($"must refer to a property or properties of '{columnsExpression.Parameters[0].Type.Name}'.");

            return columns.Select(c => c.Column);
        }

        internal static IEnumerable<SortedColumnModel> ParseOrderedColumnSelector(LambdaExpression columnsExpression, IEnumerable<ColumnModel> tableColumns)
        {
            var columns = InternalParseColumnSelector(columnsExpression, tableColumns).ToList();

            return columns.Select(c => new SortedColumnModel(c.Column, c.Order ?? SortOrder.Ascending));
        }
    }
}
