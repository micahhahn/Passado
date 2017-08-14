using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

using Passado.Model.Table;

namespace Passado.Model
{
    public static class BuilderHelper
    {
        public static string GenerateKeyName(string prefix, string schema, string tableName, IEnumerable<string> columnNames)
        {
            return $"{prefix}_{(schema != null ? schema + "_" : "")}{tableName}__{string.Join("_", columnNames)}";
        }

        public static string GenerateForeignKeyName(string schema, string tableName, IEnumerable<string> columnNames, string foreignSchema, string foreignTableName)
        {
            return $"FK_{(schema != null ? schema + "_" : "")}{tableName}__{string.Join("_", columnNames)}__{(foreignSchema != null ? foreignSchema + "_" : "")}{foreignTableName}";
        }

        public static string GetTableName(string schema, string table)
        {
            if (schema == null)
                return table;
            else
                return $"{schema}.{table}";
        }
    }
}
