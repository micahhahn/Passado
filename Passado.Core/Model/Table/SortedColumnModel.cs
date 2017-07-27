using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core.Model.Table
{
    public class SortedColumnModel : ColumnModel
    {
        public SortedColumnModel(ColumnModel model, SortOrder sortOrder)
            : base(name: model.Name,
                   propertyName: model.PropertyName, 
                   sqlType: model.SqlType,
                   isNullable: model.IsNullable, 
                   isIdentity: model.IsIdentity, 
                   defaultValue: model.DefaultValue)
        {
            SortOrder = SortOrder;
        }

        public SortOrder SortOrder { get; }
    }
}
