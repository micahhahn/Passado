using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Xunit;

using Passado.Model;
using Passado.Model.Database;

namespace Passado.Tests.Query
{
    public class EmptyDatabase
    {
        [DatabaseModelProvider]
        public static DatabaseModel ProvideModel(DatabaseBuilder<EmptyDatabase> databaseBuilder)
        {
            return databaseBuilder.Database(nameof(EmptyDatabase))
                                  .Build();
        }
    }

    public abstract class SelectQueryTests
    {
        public abstract IQueryBuilder<TDatabase> GetQueryBuilder<TDatabase>();

        [Fact]
        public void Closure_Should_Evaluate_Current_Variable_Value()
        {
            var qb = GetQueryBuilder<EmptyDatabase>();

            int i = 0;

            var query = qb.Select(t => new { I = i })
                          .Build();

            while (i < 5)
            {
                var rows = query.Execute();

                Assert.Equal(1, rows.Count());
                Assert.Equal(i, rows.Single().I);

                i++;
            }
        }

        [Fact]
        public void Closure_Should_Work_If_Referenced_Twice()
        {
            var qb = GetQueryBuilder<EmptyDatabase>();

            int i = 0;

            var query = qb.Select(t => new { I1 = i, I2 = i })
                          .Build();

            var rows = query.Execute();

            Assert.Equal(1, rows.Count());
            Assert.Equal(i, rows.Single().I1);
            Assert.Equal(i, rows.Single().I2);
        }
        
        class SelectObject
        {
            public SelectObject()
            { }

            public SelectObject(int x)
            {
                X = x;
            }

            public SelectObject(int x, int y)
            {
                X = x;
                Y = y;
            }

            public static SelectObject Create(int x, int y)
            {
                return new SelectObject(x, y);
            }

            public int X { get; set; }
            public int Y { get; set; }
        }

        [Fact]
        public void Select_Should_Work_With_Constructor()
        {
            var qb = GetQueryBuilder<EmptyDatabase>();

            var query = qb.Select(t => new SelectObject(1, 2))
                          .Build();

            var rows = query.Execute();

            Assert.Equal(1, rows.Count());
            Assert.Equal(1, rows.Single().X);
            Assert.Equal(2, rows.Single().Y);
        }

        [Fact]
        public void Select_Should_Work_With_Constructor_And_Initializers()
        {
            var qb = GetQueryBuilder<EmptyDatabase>();

            var query = qb.Select(t => new SelectObject(1) { Y = 2 })
                          .Build();

            var rows = query.Execute();

            Assert.Equal(1, rows.Count());
            Assert.Equal(1, rows.Single().X);
            Assert.Equal(2, rows.Single().Y);
        }

        [Fact]
        public void Select_Should_Work_With_Initializers()
        {
            var qb = GetQueryBuilder<EmptyDatabase>();

            var query = qb.Select(t => new SelectObject() { X = 1, Y = 2 })
                          .Build();

            var rows = query.Execute();

            Assert.Equal(1, rows.Count());
            Assert.Equal(1, rows.Single().X);
            Assert.Equal(2, rows.Single().Y);
        }

        [Fact]
        public void Select_Should_Work_With_Static_Method()
        {
            var qb = GetQueryBuilder<EmptyDatabase>();

            var query = qb.Select(t => SelectObject.Create(1, 2))
                          .Build();

            var rows = query.Execute();

            Assert.Equal(1, rows.Count());
            Assert.Equal(1, rows.Single().X);
            Assert.Equal(2, rows.Single().Y);
        }

        [Fact]
        public void Select_Should_Work_With_Anonymous_Object()
        {
            var qb = GetQueryBuilder<EmptyDatabase>();

            var query = qb.Select(t => new { X = 1, Y = 2 })
                          .Build();

            var rows = query.Execute();

            Assert.Equal(1, rows.Count());
            Assert.Equal(1, rows.Single().X);
            Assert.Equal(2, rows.Single().Y);
        }

        class SelectObjectBadName
        {
            public SelectObjectBadName(int X)
            {
                Y = X;
            }

            public int X { get; set; }
            public int Y { get; set; }
        }

        [Fact]
        public void Select_Should_Work_If_Constructor_And_Init_Share_Name()
        {
            var qb = GetQueryBuilder<EmptyDatabase>();

            var query = qb.Select(t => new SelectObjectBadName(2) { X = 1 })
                          .Build();

            var rows = query.Execute();

            Assert.Equal(1, rows.Count());
            Assert.Equal(1, rows.Single().X);
            Assert.Equal(2, rows.Single().Y);
        }
    }
}
