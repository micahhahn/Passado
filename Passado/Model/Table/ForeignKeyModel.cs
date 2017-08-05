using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Passado.Model.Table
{
    public class ForeignKeyModel
    {
        private PropertyInfo _referenceTable;
        private ImmutableArray<PropertyInfo> _referenceColumns;

        public ForeignKeyModel(string name,
                               ImmutableArray<ColumnModel> keyColumns,
                               PropertyInfo referenceTable,
                               ImmutableArray<PropertyInfo> referenceColumns,
                               ForeignKeyAction updateAction,
                               ForeignKeyAction deleteAction)
        {
            Name = name;
            KeyColumns = keyColumns;
            _referenceTable = referenceTable;
            _referenceColumns = referenceColumns;
            UpdateAction = updateAction;
            DeleteAction = deleteAction;
        }

        public void FreezeReference(IEnumerable<TableModel> tables)
        {
            ReferenceTable = tables.FirstOrDefault(t => t.Property.Name == _referenceTable.Name);

            if (ReferenceTable == null)
                throw new NotImplementedException();

            ReferenceColumns = _referenceColumns.MatchColumns(ReferenceTable.Columns).ToImmutableArray();
        }

        public string Name { get; }
        public ImmutableArray<ColumnModel> KeyColumns { get; }
        public TableModel ReferenceTable { get; private set; }
        public ImmutableArray<ColumnModel> ReferenceColumns { get; private set; }
        public ForeignKeyAction UpdateAction { get; }
        public ForeignKeyAction DeleteAction { get; }
    }
}
