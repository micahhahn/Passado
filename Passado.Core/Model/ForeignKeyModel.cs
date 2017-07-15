using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;

namespace Passado.Core.Model
{
    public class ForeignKeyModel
    {
        public string Name { get; set; }
        public ImmutableList<ColumnModel> KeyColumns { get; set; }
        public TableModel ReferenceTable { get; set; }
        public ImmutableList<ColumnModel> ReferenceColumns { get; set; }
        public ForeignKeyAction UpdateAction { get; set; }
        public ForeignKeyAction DeleteAction { get; set; }

        internal LambdaExpression ReferenceTableExpression { get; set; }
        internal LambdaExpression ReferenceColumnsExpression { get; set; }
    }
}
