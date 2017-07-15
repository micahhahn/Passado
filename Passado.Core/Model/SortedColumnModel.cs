using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core.Model
{
    public class SortedColumnModel : ColumnModel
    {
        private readonly SortOrder _sortOrder;

        public SortedColumnModel(ColumnModel model, SortOrder sortOrder)
            : base(model.PropertyName, model.ColumnName, model.SqlType, model.IsNullable, model.IsIdentity, model.DefaultValue)
        {
            _sortOrder = SortOrder;
        }

        public SortOrder SortOrder => _sortOrder;
    }
}
