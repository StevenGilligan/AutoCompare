using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AutoCompare.Tests
{
    [TestClass]
    public class ConfigurationTests : AutoCompareBaseTest
    {
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Configure_A_Type_Twice_Should_Throw()
        {
            SutEngine.Configure<SimpleModel>()
                .Ignore(x => x.Value);

            SutEngine.Configure<SimpleModel>();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Configure_A_Property_Twice_Should_Throw()
        {
            SutEngine.Configure<SimpleModel>()
                .For(x => x.Value, x => x.Ignore())
                .For(x => x.Value, x => x.Ignore());

            SutEngine.Configure<NestedList>()
                .For(x => x.Children, x => x.MatchUsing(y => y.Id));
        }
    }
}
