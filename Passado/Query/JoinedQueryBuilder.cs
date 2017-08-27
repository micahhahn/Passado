using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Passado.Query
{
    public class JoinedQueryBuilder<TDatabase, TTable1>
        : Select.IFromQuery<TDatabase, TTable1>
        , Update.IUpdateQuery<TDatabase, TTable1>
        , Delete.IDeleteQuery<TDatabase, TTable1>
                                                        
    {
        
    }

    public class JoinedQueryBuilder<TDatabase, TTable1, TTable2>
        : Select.IJoinQuery<TDatabase, TTable1, TTable2>
        , Update.IJoinQuery<TDatabase, TTable1, TTable2>
        , Delete.IJoinQuery<TDatabase, TTable1, TTable2>
    {

    }
}
