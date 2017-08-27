using System;

namespace Passado.Query.Select
{
    public interface IGroupByQuery<TGroupedRow>
        : IHavingable<TGroupedRow>
        , ISelectable<TGroupedRow>                                  
    {

    }
}
