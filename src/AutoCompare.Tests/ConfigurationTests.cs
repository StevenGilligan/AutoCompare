using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AutoCompare.Tests
{
    [TestClass]
    public class ConfigurationTests : AutoCompareBaseTest
    {
        [TestMethod]
        [ExpectedException(typeof(Exception), "The type CompileTwice is already configured.")]
        public void Configure_A_Type_Twice_Should_Throw()
        {
            SutEngine.Configure<SimpleModel>()
                .Ignore(x => x.Value);

            SutEngine.Configure<SimpleModel>();
        }
    }
}
