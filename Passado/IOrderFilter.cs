using System;
using System.Collections.Generic;
using System.Text;

namespace Passado
{
    public interface IOrderFilter<T>
    {
        T Asc { get; }
        T Desc { get; }
    }
}
