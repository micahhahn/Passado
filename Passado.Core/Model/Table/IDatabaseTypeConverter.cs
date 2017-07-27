using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core.Model.Table
{
    public interface IDatabaseTypeConverter<TType>
    {
        object ConvertTo(TType @object);
        TType ConvertFrom(object @object);
    }
}
