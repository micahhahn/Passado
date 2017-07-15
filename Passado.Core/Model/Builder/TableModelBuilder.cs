using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Passado.Core.Model.Builder
{
    public class TableModelBuilder<TDatabase>
    {
        public ColumnModelBuilder<TDatabase, TTable> Table<TTable>(Expression<Func<TDatabase, IEnumerable<TTable>>> tableSelector, string name = null, string schema = null)
        {
            var propertyName = Builder.ParsePropertySelector(tableSelector);

            if (propertyName == null)
                throw new Exception();

            return new ColumnModelBuilder<TDatabase, TTable>()
            {
                Name = name ?? propertyName,
                Schema = schema,
                PropertyName = propertyName,
                PropertyType = typeof(TTable)
            };
        }
    }
}
