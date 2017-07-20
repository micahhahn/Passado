﻿using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

using Passado.Core.Model;

namespace Passado.Analyzers.Model
{
    public class FuzzyColumnModel
    {
        public Optional<string> Name { get; set; }
        public Optional<FuzzyProperty> Property { get; set; }
        public Optional<SqlType> Type { get; set; }
        public Optional<bool> IsNullable { get; set; }
        public Optional<object> DefaultValue { get; set; }
        public Optional<bool> IsIdentity { get; set; }
    }
}