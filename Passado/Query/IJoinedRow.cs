namespace Passado.Query
{
    public interface IJoinedRow<TTable1>
    {
        TTable1 T1 { get; }
    }

    public interface IJoinedRow<TTable1, TTable2> : IJoinedRow<TTable1>
    {
        TTable2 T2 { get; }
    }

    public interface IJoinedRow<TTable1, TTable2, TTable3> : IJoinedRow<TTable1, TTable2>
    {
        TTable3 T3 { get; }
    }

    public interface IJoinedRow<TTable1, TTable2, TTable3, TTable4> : IJoinedRow<TTable1, TTable2, TTable3>
    {
        TTable4 T4 { get; }
    }
}
