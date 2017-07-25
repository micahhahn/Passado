using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

using Passado.Core.Model;

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


    }
}
