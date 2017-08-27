
namespace Passado.Query.Select
{
    public interface IWhereQuery<TJoinedRow>
        : IGroupable<TJoinedRow>
        , ISelectable<TJoinedRow>
    {

    }
}
