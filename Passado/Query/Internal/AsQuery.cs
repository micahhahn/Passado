using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Reflection;

using Passado.Error;

namespace Passado.Query.Internal
{
    public abstract class AsQueryBase : QueryBase
    {
        public ImmutableArray<(string DefaultName, string AsName)> Names { get; set; }
    }

    public class AsQuery<TNamedRow, TTable1>
       : AsQueryBase
       , Select.IAsQuery<TNamedRow>
       , Update.IAsQuery<TNamedRow, TTable1>
       , Delete.IAsQuery<TNamedRow>
    {
        public AsQuery(QueryBase innerQuery, LambdaExpression name)
        {
            InnerQuery = innerQuery;

            if (name == null)
                throw BuilderError.ArgumentNull(nameof(name)).AsException();

            if (name.Body is NewExpression newExpression)
            {
                Names = newExpression.Arguments.Zip(newExpression.Members, (l, r) => ((l as MemberExpression).Member.Name, r.Name))
                                               .ToImmutableArray();
            }
            else
            {
                throw new NotImplementedException();
            }
        }   
    }
}
