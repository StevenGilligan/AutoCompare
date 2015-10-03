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

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Configure_Enumerable_Twice_Should_Throw()
        {
            SutEngine.Configure<ArrayModel>()
                .For(x => x.ArrayChildren, x => x.MatchUsing(y => y.Id))
                .For(x => x.ArrayChildren, x => x.Ignore());
        }

        [TestMethod]
        public void Configure_Array()
        {
            SutEngine.Configure<ArrayModel>()
                .For(x => x.ArrayChildren, x => x.MatchUsing(y => y.Id));

            var oldModel = new ArrayModel()
            {
                Id = 1,
                ArrayChildren = new[]
                {
                    new GrandChildModel()
                    {
                        Id = 100,
                        Name = "Name 1",
                        Value = 100
                    },
                    new GrandChildModel()
                    {
                        Id = 200,
                        Name = "Name 2",
                        Value = 200
                    }
                },
            };

            var newModel = new ArrayModel()
            {
                Id = 1,
                ArrayChildren = new[]
                {
                    new GrandChildModel()
                    {
                        Id = 100,
                        Name = "Name 1",
                        Value = 100
                    },
                    new GrandChildModel()
                    {
                        Id = 300,
                        Name = "Name 3",
                        Value = 300
                    }
                },
            };

            var diff = SutEngine.Compare(oldModel, newModel);
            Assert.AreEqual(6, diff.Count);
        }
    }
}
