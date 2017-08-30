using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Passado.Model;

namespace Passado.Query
{
    public class JoinedTable
    {
        public string Name { get; set; }
        public JoinType JoinType { get; set; }
        public TableModel Model { get; set; }
        public Expression Condition { get; set; }
    }

    public abstract class JoinedQueryBuilder
    {
        public JoinedQueryBuilder((DatabaseModel, ImmutableArray<JoinedTable>) args)
        {
            DatabaseModel = args.Item1;
            JoinedTables = args.Item2;
        }

        public DatabaseModel DatabaseModel { get; }
        public ImmutableArray<JoinedTable> JoinedTables { get; }
    }

    public class JoinedQueryBuilder<TDatabase>
        : JoinedQueryBuilder
    {
        public JoinedQueryBuilder((DatabaseModel, ImmutableArray<JoinedTable>) args)
            : base(args)
        { }
    }

    public class JoinedQueryBuilder<TDatabase, TTable1>
        : JoinedQueryBuilder
        , Select.IFromQuery<TDatabase, TTable1>
        , Update.IUpdateQuery<TDatabase, TTable1>
        , Delete.IDeleteQuery<TDatabase, TTable1>
    {
        public JoinedQueryBuilder((DatabaseModel, ImmutableArray<JoinedTable>) args)
            : base(args)
        { }
    }

    public class JoinedQueryBuilder<TDatabase, TTable1, TTable2>
        : JoinedQueryBuilder
        , Select.IJoinQuery<TDatabase, TTable1, TTable2>
        , Update.IJoinQuery<TDatabase, TTable1, TTable2>
        , Delete.IJoinQuery<TDatabase, TTable1, TTable2>
    {
        public JoinedQueryBuilder((DatabaseModel, ImmutableArray<JoinedTable>) args)
            : base(args)
        { }
    }

    public class JoinedQueryBuilder<TDatabase, TTable1, TTable2, TTable3>
        : JoinedQueryBuilder
        , Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3>
        , Update.IJoinQuery<TDatabase, TTable1, TTable2, TTable3>
        , Delete.IJoinQuery<TDatabase, TTable1, TTable2, TTable3>
    {
        public JoinedQueryBuilder((DatabaseModel, ImmutableArray<JoinedTable>) args)
                    : base(args)
        { }
    }
}
