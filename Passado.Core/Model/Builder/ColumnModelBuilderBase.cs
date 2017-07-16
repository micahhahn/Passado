﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Passado.Core.Model.Builder
{
    public abstract class ColumnModelBuilderBase<TDatabase, TTable>
    {
        public string Name { get; set; }
        public string Schema { get; set; }

        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }

        public ColumnOrPrimaryKeyBuilder<TDatabase, TTable> Column<TColumn>(Expression<Func<TTable, TColumn>> columnSelector, SqlType type, bool nullable = false, string name = null, bool identity = false)
        {
            var propertyName = Builder.ParsePropertySelector(columnSelector);

            if (propertyName == null)
                throw new ModelException($"{Builder.GetTablePrefix<TTable>()}{nameof(columnSelector)}' must refer to a property of '{typeof(TTable).Name}'.");

            var newColumnModel = new ColumnModel(propertyName: propertyName,
                                                 columnName: name ?? propertyName,
                                                 sqlType: type,
                                                 isNullable: nullable,
                                                 isIdentity: identity,
                                                 defaultValue: null);

            if (this as ColumnModelBuilder<TDatabase, TTable> != null)
            {
                return new ColumnOrPrimaryKeyBuilder<TDatabase, TTable>()
                {
                    Name = Name,
                    Schema = Schema,
                    PropertyName = PropertyName,
                    PropertyType = PropertyType,
                    Columns = new List<ColumnModel>()
                    {
                        newColumnModel
                    }
                };
            }
            else
            {
                var temp = this as ColumnOrPrimaryKeyBuilder<TDatabase, TTable>;
                temp.Columns.Add(newColumnModel);
                return temp;
            }
        }
    }
}