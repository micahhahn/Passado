using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;

namespace Passado.Model.Table
{
    public class ForeignKeyModel
    {
        private LambdaExpression _referenceTableExpression;
        private LambdaExpression _referenceColumnsExpression;

        public ForeignKeyModel(string name,
                               ImmutableList<ColumnModel> keyColumns,
                               LambdaExpression referenceTableExpression,
                               LambdaExpression referenceColumnsExpression,
                               ForeignKeyAction updateAction,
                               ForeignKeyAction deleteAction)
        {
            Name = name;
            KeyColumns = keyColumns;
            _referenceTableExpression = referenceTableExpression;
            _referenceColumnsExpression = referenceColumnsExpression;
            UpdateAction = updateAction;
            DeleteAction = deleteAction;
        }

        public void FreezeReferenceExpressions(DatabaseModel model)
        {

        }

        public string Name { get; }
        public ImmutableList<ColumnModel> KeyColumns { get; }
        public TableModel ReferenceTable { get; }
        public ImmutableList<ColumnModel> ReferenceColumns { get; }
        public ForeignKeyAction UpdateAction { get; }
        public ForeignKeyAction DeleteAction { get; }
    }
}
