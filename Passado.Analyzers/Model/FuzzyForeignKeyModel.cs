using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;

using Passado.Model;

namespace Passado.Analyzers.Model
{
    public class FuzzyForeignKeyModel
    {
        public Optional<string> Name { get; set; }
        public Optional<List<FuzzyColumnModel>> KeyColumns { get; set; }
        public Optional<FuzzyTableModel> ReferenceTable { get; set; }
        public Optional<List<FuzzyColumnModel>> ReferenceColumns { get; set; }
        public Optional<ForeignKeyAction> UpdateAction { get; set; }
        public Optional<ForeignKeyAction> DeleteAction { get; set; }

        // Temporary placeholders for ReferenceTable and ReferenceColumns as the referenced table and colums
        // might not be defined at the time we parse the foriegn key.
        public Optional<(FuzzyProperty, Location)> ReferenceTableSelector { get; set; }
        public Optional<ImmutableArray<(FuzzyProperty, Location)>> ReferenceColumnSelectors { get; set; }
    }
}
