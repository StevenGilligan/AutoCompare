using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace AutoCompare.Tests
{
    [TestClass]
    public partial class IoCTests : AutoCompareBaseTest
    {
        [TestMethod]
        public void Two_Engines_Can_Run_At_The_Same_Time()
        {
            var engine1 = SutEngine;
            engine1.Configure<SimpleModel>()
                .Ignore(x => x.Date)
                .Compile.Async();

            IComparerEngine engine2 = new Engine();
            engine2.Configure<SimpleModel>()
                .Ignore(x => x.Value)
                .Compile.Async();

            var obj1 = new SimpleModel
            {
                Id = 1,
                Name = "Name",
                Date = new DateTime(2015, 1, 1),
                Value = 3.14m
            };

            var obj2 = new SimpleModel
            {
                Id = 2,
                Name = "Name2",
                Date = new DateTime(2015, 1, 2),
                Value = 1m
            };

            var diff1 = engine1.Compare(obj1, obj2);
            var diff2 = engine2.Compare(obj1, obj2);

            Assert.AreEqual(diff1.Count, diff2.Count);
            Assert.AreEqual(diff1.First(x => x.Name == "Id"), diff2.First(x => x.Name == "Id"));
            Assert.AreEqual(diff1.First(x => x.Name == "Name"), diff2.First(x => x.Name == "Name"));
            Assert.IsNotNull(diff1.FirstOrDefault(x => x.Name == "Value"));
            Assert.IsNotNull(diff2.FirstOrDefault(x => x.Name == "Date"));
            Assert.IsNull(diff1.FirstOrDefault(x => x.Name == "Date"));
            Assert.IsNull(diff2.FirstOrDefault(x => x.Name == "Value"));
        }
    }
}
