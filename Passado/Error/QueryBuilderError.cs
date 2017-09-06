using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Passado.Model;

namespace Passado.Error
{
    public class QueryBuilderError : BuilderError
    {
        private QueryBuilderError(string errorId, string title, string message)
            : base(errorId, title, message)
        {

        }

        public static QueryBuilderError DatabaseTypeNoModelProvider(string typeName) => new QueryBuilderError("PSxxxx", "Database Type Must Contain a Model Provider", $"'{typeName}' must have one public static function marked with the '{nameof(DatabaseModelProviderAttribute)}' to be used as a database.");
        public static QueryBuilderError DatabaseTypeModelProviderIncorrectType(string typeName) => new QueryBuilderError("PSxxxx", "Database Type Model Provider Incorrect Type", $"The database model provider function for '{typeName}' must be of type 'Func<DatabaseModel, DatabaseModelBuilder<{typeName}>>'.");

        // Join Errors
        public static QueryBuilderError JoinNoForeignKeysForImplicitCondition(string tableName, IEnumerable<string> joinedTableNames) => new QueryBuilderError("PSxxxx", "No Foreign Keys Found for Implicit Join Condition", $"There must be at least one foreign key reference between '{tableName}' and {(joinedTableNames.Count() == 1 ? $"'{joinedTableNames.First()}'" : $"one of '{string.Join("', '", joinedTableNames)}'")} in order to infer an implicit join condition.");
        public static QueryBuilderError JoinMultipleForeignKeysForImplicitCondition(IEnumerable<string> foreignKeyNames) => new QueryBuilderError("PSxxxx", "Multiple Foreign Keys Found for Implicit Join Condition", $"Cannot infer which of ['{string.Join("', '", foreignKeyNames)}'] should be used for implicit join condition.");
    }
}
