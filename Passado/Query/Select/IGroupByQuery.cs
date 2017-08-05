using System;

namespace Passado.Query.Select
{
    public interface IGroupByQuery<TGroupedRow> : ISelectable<TGroupedRow>,
                                                  IHavingable<TGroupedRow>
    {

    }
}
