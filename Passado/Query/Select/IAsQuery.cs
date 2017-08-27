using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Select
{
    public interface IAsQuery<TJoinedRow>
        : IWhereable<TJoinedRow>
        , IGroupable<TJoinedRow>
        , ISelectable<TJoinedRow>   
    {
    }
}
