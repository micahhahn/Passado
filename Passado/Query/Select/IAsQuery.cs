using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Select
{
    public interface IAsQuery<TJoinedRow> : ISelectable<TJoinedRow>
                                          , IWhereable<TJoinedRow>
                                          , IGroupable<TJoinedRow>
    {
    }
}
