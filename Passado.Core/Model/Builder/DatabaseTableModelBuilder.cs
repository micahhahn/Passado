using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Passado.Core.Model.Builder
{
    public class DatabaseTableModelBuilder<TDatabase>
    {
        internal string Name { get; set; }
        internal List<TableModel> Tables { get; set; }

        public DatabaseTableModelBuilder<TDatabase> Table(Func<TableModelBuilder<TDatabase>, TableModel> buildTableModel)
        {
            try
            {
                var tableModel = buildTableModel(new TableModelBuilder<TDatabase>());

                if (Tables.Any(t => t.PropertyName == tableModel.PropertyName))
                    throw new ModelException($"{Builder.GetDatabasePrefix<TDatabase>()}Table '{tableModel.PropertyName}' can only be referenced once.");

                if (Tables.Any(t => t.Name == tableModel.Name))
                    throw new ModelException($"{Builder.GetDatabasePrefix<TDatabase>()}Table name '{tableModel.Name}' can only be used once.");

                Tables.Add(tableModel);

                return this;
            }
            catch (ModelException exception)
            {
                throw new ModelException($"{Builder.GetDatabasePrefix<TDatabase>()}{exception.Message}");
            }
        }

        public DatabaseModel Build()
        {
            // Establish and verify all foreign key relationships.
            foreach (var table in Tables)
            {
                foreach (var foreignKey in table.ForeignKeys)
                {
                    var tablePropertyName = Builder.ParseWeakPropertySelector(foreignKey.ReferenceTableExpression);

                    if (tablePropertyName == null)
                        throw new Exception();

                    foreignKey.ReferenceTable = Tables.SingleOrDefault(t => t.PropertyName == tablePropertyName);

                    if (foreignKey.ReferenceTable == null)
                        throw new Exception();

                    foreignKey.ReferenceColumns = Builder.ParseColumnSelector(foreignKey.ReferenceColumnsExpression, foreignKey.ReferenceTable.Columns).ToImmutableList();

                    foreignKey.Name = foreignKey.Name ?? $"FK_{(table.Schema != null ? $"{table.Schema}_" : "")}{table.Name}_{(foreignKey.ReferenceTable.Schema != null ? $"{foreignKey.ReferenceTable.Schema}_" : "")}{foreignKey.ReferenceTable.Name}";

                    foreignKey.ReferenceTableExpression = null;
                    foreignKey.ReferenceColumnsExpression = null;
                }
            }

            return new DatabaseModel(name: Name,
                                     tables: Tables);
        }
    }
}
