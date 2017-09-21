using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Passado.Model;
using Passado.Model.Table;

namespace Passado.Migration
{
    public static class Migration
    {
        public static string CreateDatabase(DatabaseModel model)
        {
            var databaseSql = $"CREATE DATABASE \"Mock{model.Name}\";";

            // Order tables by FK constraints.
            var tablesSql = SortTablesByGraph(model.Tables).Select(t => CreateTableSql(t));

            return string.Join("\n\n", /*(new List<string>() { databaseSql }).Union(*/tablesSql/*)*/);
        }

        static string CreateColumnTypeSql(SqlType type)
        {
            switch (type)
            {
                case SqlType.Int:
                    return "int";
                case SqlType.String:
                    return "nvarchar(max)";
            }

            throw new NotImplementedException();
        }

        static string CreateTableSql(TableModel tableModel)
        {
            var tableName = GetTableName(tableModel);
            var columnsSql = string.Join(",\n\t", tableModel.Columns.Select(CreateColumnSql));
            
            var keys = new List<string>();

            if (tableModel.PrimaryKey != null)
            {
                keys.Add($"CONSTRAINT [{tableModel.PrimaryKey.Name}] PRIMARY KEY{(tableModel.PrimaryKey.IsClustered ? " CLUSTERED" : "")} ({OrderedColumnListToSql(tableModel.PrimaryKey.Columns)})");
            }

            keys.AddRange(tableModel.ForeignKeys.Select(f => $"CONSTRAINT [{f.Name}] FOREIGN KEY ({ColumnListToSql(f.KeyColumns)}) REFERENCES {GetTableName(f.ReferenceTable)} ({ColumnListToSql(f.ReferenceColumns)}) ON UPDATE {GetForeignKeyAction(f.UpdateAction)} ON DELETE {GetForeignKeyAction(f.DeleteAction)}"));

            var statements = new List<string>() { $"CREATE TABLE {tableName} (\n\t{columnsSql}\n\t{string.Join(",\n\t", keys)}\n);" };

            statements.AddRange(tableModel.Indexes.Select(i => $"CREATE {(i.IsUnique ? "UNIQUE " : "")}{(i.IsClustered ? "CLUSTERED " : "")}INDEX [{i.Name}] ON {GetTableName(tableModel)} ({OrderedColumnListToSql(i.KeyColumns)}){(i.IncludedColumns != null ? $" INCLUDE ({ColumnListToSql(i.IncludedColumns)})" : "")};"));

            return string.Join("\n\n", statements);
        }

        static string ColumnListToSql(IEnumerable<ColumnModel> columns)
        {
            return string.Join(", ", columns.Select(c => $"[{c.Name}]"));
        }

        static string OrderedColumnListToSql(IEnumerable<SortedColumnModel> columns)
        {
            // If all columns are ascending - which is by far the most common case - don't bother putting ASC after every one.
            if (columns.All(c => c.SortOrder == SortOrder.Ascending))
                return ColumnListToSql(columns);

            return string.Join(", ", columns.Select(c => $"[{c.Name}] {(c.SortOrder == SortOrder.Ascending ? "ASC" : "DESC")}"));
        }

        static string GetForeignKeyAction(ForeignKeyAction action)
        {
            switch (action)
            {
                case ForeignKeyAction.NoAction:
                    return "NO ACTION";
                case ForeignKeyAction.Cascade:
                    return "CASCADE";
                case ForeignKeyAction.SetNull:
                    return "SET NULL";
                case ForeignKeyAction.SetDefault:
                    return "SET DEFAULT";
                default:
                    throw new NotImplementedException();
            }
        }

        static string GetTableName(TableModel model)
        {
            return $"{(model.Schema != null ? $"[{model.Schema}]" : "")}[{model.Name}]";
        }

        static string CreateColumnSql(ColumnModel columnModel)
        {
            var nullString = columnModel.IsNullable ? "NULL" : "NOT NULL";

            return $"[{columnModel.Name}] {CreateColumnTypeSql(columnModel.SqlType)} {nullString}{(columnModel.IsIdentity ? " IDENTITY(1, 1)" : "")}";
        }

        static int GetTableRank(TableModel model, Dictionary<string, (TableModel, int)> orderedTables)
        {
            int maxRank = 0;

            foreach (var key in model.ForeignKeys)
            {
                var tableName = GetTableName(key.ReferenceTable);

                if (orderedTables.TryGetValue(tableName, out (TableModel, int) value))
                {
                    maxRank = Math.Max(maxRank, value.Item2 + 1);
                }
                else
                {
                    var rank = GetTableRank(key.ReferenceTable, orderedTables);
                    orderedTables.Add(tableName, (key.ReferenceTable, rank));
                    maxRank = Math.Max(maxRank, rank + 1);
                }
            }

            return maxRank;
        }

        static IEnumerable<TableModel> SortTablesByGraph(IEnumerable<TableModel> tables)
        {
            var orderedTables = new Dictionary<string, (TableModel, int)>();

            foreach (var table in tables)
            {
                var tableName = GetTableName(table);

                if (!orderedTables.ContainsKey(tableName))
                {
                    var rank = GetTableRank(table, orderedTables);

                    orderedTables.Add(tableName, (table, rank));
                }
            }

            return orderedTables.Values
                                .OrderBy(v => v.Item2)
                                .ThenBy(v => v.Item1.Schema)
                                .ThenBy(v => v.Item1.Name)
                                .Select(v => v.Item1);
        }
    }
}
