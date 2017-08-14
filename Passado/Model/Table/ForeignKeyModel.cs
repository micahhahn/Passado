using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Passado.Error;

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

        public void FreezeReference(TableModel table, IEnumerable<TableModel> tables)
        {
            ReferenceTable = tables.FirstOrDefault(t => t.Property.Name == _referenceTable.Name);

            if (ReferenceTable == null)
                throw BuilderError.SelectorNotMappedToTable(_referenceTable.Name, _referenceTable.DeclaringType.Name).AsException();

            ReferenceColumns = _referenceColumns.MatchColumns(ReferenceTable.Name, ReferenceTable.Columns).ToImmutableArray();

            // If the name was not explicitly set then we generate one based on the table and column names
            if (Name == null)
                Name = BuilderHelper.GenerateForeignKeyName(table.Schema, table.Name, KeyColumns.Select(c => c.Name), ReferenceTable.Schema, ReferenceTable.Name);
        }

        public string Name { get; private set; }
        public ImmutableArray<ColumnModel> KeyColumns { get; }
        public TableModel ReferenceTable { get; private set; }
        public ImmutableArray<ColumnModel> ReferenceColumns { get; private set; }
        public ForeignKeyAction UpdateAction { get; }
        public ForeignKeyAction DeleteAction { get; }
    }
}
