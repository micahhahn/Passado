using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

namespace Passado.Query.Internal
{
    public abstract class AsQueryBase : QueryBase
    {
        public Expression Name { get; protected set; }
    }

    public class AsQuery<TNamedRow, TTable1>
       : AsQueryBase
       , Select.IAsQuery<TNamedRow>
       , Update.IAsQuery<TNamedRow, TTable1>
       , Delete.IAsQuery<TNamedRow>
    {
        public AsQuery(QueryBase innerQuery, Expression name)
        {
            InnerQuery = innerQuery;
            Name = name;
        }   
    }
}
