﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Reflection;

using Passado.Model;
using Passado.Error;

namespace Passado.Query.Internal
{
    public abstract class JoinQueryBase : QueryBase
    {
        public JoinQueryBase(QueryBase innerQuery, JoinType joinType, LambdaExpression table, string defaultName)
        {
            InnerQuery = innerQuery;
            JoinType = joinType;
            DefaultName = defaultName;

            var property = ExpressionHelpers.ParseSelector(table);
            Model = GetQueryBuilderBase(innerQuery).DatabaseModel.Tables.First(t => t.Property.Name == property.Name);

            Condition = MakeImplicitJoinCondition(GetJoinedTables(innerQuery), Model);
        }

        public JoinQueryBase(QueryBase innerQuery, JoinType joinType, LambdaExpression table, string defaultName, LambdaExpression condition)
        {
            InnerQuery = innerQuery;
            JoinType = joinType;
            Condition = condition;
            DefaultName = defaultName;

            var property = ExpressionHelpers.ParseSelector(table);
            Model = GetQueryBuilderBase(innerQuery).DatabaseModel.Tables.First(t => t.Property.Name == property.Name);
        }
        
        public string DefaultName { get; }
        public JoinType JoinType { get; }
        public TableModel Model { get; }
        public LambdaExpression Condition { get; }
        
        static QueryBuilderBase GetQueryBuilderBase(QueryBase query)
        {
            if (query is FromQueryBase fromQuery)
                return fromQuery.QueryBuilderBase;
            else
                return GetQueryBuilderBase(query.InnerQuery);
        }

        static ImmutableArray<(string DefaultName, TableModel Model)> GetJoinedTables(QueryBase query)
        {
            if (query is FromQueryBase fromQuery)
            {
                return ImmutableArray.Create((fromQuery.Name, fromQuery.Model));
            }
            else if (query is JoinQueryBase joinQuery)
            {
                return GetJoinedTables(joinQuery.InnerQuery).Add((joinQuery.DefaultName, joinQuery.Model));
            }
            else
            {
                return GetJoinedTables(query.InnerQuery);
            }
        }

        static PropertyInfo FindJoinedRowProperty(IEnumerable<Type> joinedTableTypes, string propertyName)
        {
            return MakeJoinedRowType(joinedTableTypes).GetTypeInfo().GetProperty(propertyName) ?? FindJoinedRowProperty(joinedTableTypes.Take(joinedTableTypes.Count() - 1), propertyName);
        }

        static Type MakeJoinedRowType(IEnumerable<Type> tableTypes)
        {
            // Hackish unless there is a better way to specify generic airity at runtime.
            var openType = typeof(IJoinedRow<>).GetTypeInfo().Assembly.GetType($"{nameof(Passado)}.{nameof(Query)}.{nameof(IJoinedRow<object>)}`{tableTypes.Count()}");
            return openType.MakeGenericType(tableTypes.ToArray());
        }

        static LambdaExpression MakeImplicitJoinCondition(ImmutableArray<(string DefaultName, TableModel Model)> preJoinedTables, TableModel newTable)
        {
            var newTableName = $"T{preJoinedTables.Length + 1}";

            // Find all foreign keys in previously joined tables that reference this table
            var foreignKeysTo = preJoinedTables.SelectMany(t => t.Model
                                                                 .ForeignKeys
                                                                 .Where(f => f.ReferenceTable.Property.Name == newTable.Property.Name)
                                                                 .Select(f => new
                                                                 {
                                                                     Name = f.Name,
                                                                     FromTable = (Name: t.DefaultName, Model: t.Model),
                                                                     FromColumns = f.KeyColumns,
                                                                     ToTable = (Name: newTableName, Model: newTable),
                                                                     ToColumns = f.ReferenceColumns
                                                                 }));

            // Find all foreign keys in this table that reference previously joined tables
            var foreignKeysFrom = newTable.ForeignKeys
                                          .Select(f => new
                                          {
                                              ForeignKey = f,
                                              JoinQuery = preJoinedTables.Select(p => p as (string DefaultName, TableModel Model)?)
                                                                         .FirstOrDefault(t => t?.Model.Property.Name == newTable.Property.Name)
                                          })
                                          .Where(f => f.JoinQuery != null)
                                          .Select(f => new
                                          {
                                              Name = f.ForeignKey.Name,
                                              FromTable = (Name: newTableName, Model: newTable),
                                              FromColumns = f.ForeignKey.KeyColumns,
                                              ToTable = (Name: f.JoinQuery?.DefaultName, Mode: f.JoinQuery?.Model),
                                              ToColumns = f.ForeignKey.ReferenceColumns
                                          });

            var allForeignKeys = foreignKeysTo.Union(foreignKeysFrom).ToList();

            if (allForeignKeys.Count == 0)
                throw QueryBuilderError.JoinNoForeignKeysForImplicitCondition(newTable.Name, preJoinedTables.Select(j => j.DefaultName)).AsException();

            if (allForeignKeys.Count > 1)
                throw QueryBuilderError.JoinMultipleForeignKeysForImplicitCondition(allForeignKeys.Select(f => f.Name)).AsException();

            var foreignKey = allForeignKeys.Single();

            var joinedTableTypes = preJoinedTables.Select(t => t.Model)
                                                  .Concat(new[] { newTable })
                                                  .Select(j => j.Property.Type.GenericTypeArguments[0]);
            
            var joinedRowType = MakeJoinedRowType(joinedTableTypes);
            var parameter = Expression.Parameter(joinedRowType, "t");

            var rightTable = Expression.Property(parameter, FindJoinedRowProperty(joinedTableTypes, foreignKey.ToTable.Name));
            var leftTable = Expression.Property(parameter, FindJoinedRowProperty(joinedTableTypes, foreignKey.FromTable.Name));

            var body = foreignKey.FromColumns
                                 .Zip(foreignKey.ToColumns, (l, r) => Expression.Equal(Expression.Property(leftTable, l.Property.Name),
                                                                                       Expression.Property(rightTable, r.Property.Name)))
                                 .Aggregate((l, r) => Expression.AndAlso(l, r));

            return Expression.Lambda(body, parameter);
        }
    }

    public class JoinQuery<TDatabase, TTable1, TTable2>
        : JoinQueryBase
        , Select.IJoinQuery<TDatabase, TTable1, TTable2>
        , Update.IJoinQuery<TDatabase, TTable1, TTable2>
        , Delete.IJoinQuery<TDatabase, TTable1, TTable2>
    {
        public JoinQuery(QueryBase innerQuery, JoinType joinType, LambdaExpression table)
            : base(innerQuery, joinType, table, "T2")
        { }

        public JoinQuery(QueryBase innerQuery, JoinType joinType, LambdaExpression table, LambdaExpression condition)
            : base(innerQuery, joinType, table, "T2", condition)
        { }
    }

    public class JoinQuery<TDatabase, TTable1, TTable2, TTable3>
        : JoinQueryBase
        , Select.IJoinQuery<TDatabase, TTable1, TTable2, TTable3>
        , Update.IJoinQuery<TDatabase, TTable1, TTable2, TTable3>
        , Delete.IJoinQuery<TDatabase, TTable1, TTable2, TTable3>
    {
        public JoinQuery(QueryBase innerQuery, JoinType joinType, LambdaExpression table)
            : base(innerQuery, joinType, table, "T3")
        { }

        public JoinQuery(QueryBase innerQuery, JoinType joinType, LambdaExpression table, LambdaExpression condition)
            : base(innerQuery, joinType, table, "T3", condition)
        { }
    }
}
