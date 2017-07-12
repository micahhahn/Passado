namespace Passado.Core.Query
{
    public interface IJoinedRow<TTable1>
    {
        TTable1 T1 { get; }
    }

    public interface IJoinedRow<TTable1, TTable2>
    {
        TTable1 T1 { get; }
        TTable2 T2 { get; }
    }

    public interface IJoinedRow<TTable1, TTable2, TTable3>
    {
        TTable1 T1 { get; }
        TTable2 T2 { get; }
        TTable3 T3 { get; }
    }

    public interface IJoinedRow<TTable1, TTable2, TTable3, TTable4>
    {
        TTable1 T1 { get; }
        TTable2 T2 { get; }
        TTable3 T3 { get; }
        TTable4 T4 { get; }
    }
}
