using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

using Passado.Error;
using Passado.Model.Table;

namespace Passado.Query.Internal
{
    public abstract class SetQueryBase : QueryBase
    {
        public SetQueryBase(QueryBase innerBuilder, Expression column, Expression value)
        {
            if (column == null)
                throw BuilderError.ArgumentNull(nameof(column)).AsException();

            var columnSelector = ExpressionHelpers.ParseSelector(column as LambdaExpression);

            if (value == null)
                throw BuilderError.ArgumentNull(nameof(value)).AsException();

            InnerQuery = innerBuilder;
            //Column = column;
            Value = value;
        }

        public ColumnModel Column { get; }
        public Expression Value { get; }
    }

    public class SetQuery<TRow, TTable1>
        : SetQueryBase
        , Update.ISetQuery<TRow, TTable1>
    {
        public SetQuery(QueryBase innerBuilder, Expression column, Expression value)
            : base(innerBuilder, column, value)
        {
            
        }
    }
}
