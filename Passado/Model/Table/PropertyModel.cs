using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Model.Table
{
    public class PropertyModel
    {
        public PropertyModel(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public Type Type { get; }
    }
}
