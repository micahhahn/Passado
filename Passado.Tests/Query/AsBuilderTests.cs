using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

namespace Passado.Tests.Query
{
    public class AsBuilderTests //: QueryBuilderErrorTests
    {
        [Theory]
        [InlineData("(int?)null")]
        public void Error_On_Null_Name_Selector(string selector)
        {

        }
    }
}
