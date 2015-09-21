using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace AutoCompare.Tests
{
    [TestClass]
    public partial class BuilderTests
    {
        [TestMethod]
        public void Compile_A_Child_Type()
        {
            Comparer.Configure<Child>()
                .Compile.Now();
        }

        [TestMethod]
        public void Compile_A_Type_With_Circular_References()
        {
            Comparer.Configure<ParentCirularRef>()
                .Compile.Now();
        }

        [TestMethod]
        public void Compile_Async()
        {
            // TODO: A more robust way to test this, probably will need to expose some
            // of the IComparerEngine internals
            var sw1 = new Stopwatch();
            var sw2 = new Stopwatch();

            sw1.Start();
            Comparer.Configure<Parent>()
                .Compile.Async();
            sw1.Stop();
            var async = sw1.Elapsed;
            sw2.Start();
            var comparer = Comparer.Get<Parent>();
            sw2.Stop();
            var get = sw2.Elapsed;

            // .Compile.Async will return immediately, .Get<> will block while the comparer is being compiled
            Assert.IsTrue(get > async);
        }

        [TestMethod]
        public void Compile_A_Type_With_No_Public_Properties_To_Compare()
        {
            var diff = Comparer.Compare(new NoPublicProperty(1), new NoPublicProperty(2));
            Assert.AreEqual(0, diff.Count);
        }

        [TestMethod]
        public void Compile_A_Type_With_All_Properties_Ignored()
        {
            Comparer.Configure<IgnoreAllProperties>()
                .Ignore(x => x.Id)
                .Ignore(x => x.Name);
            var diff = Comparer.Compare(new IgnoreAllProperties() { Id = 1, Name = "Name" },
                new IgnoreAllProperties() { Id = 2, Name = "Name2" });
            Assert.AreEqual(0, diff.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "The type CompileTwice is already configured.")]
        public void Compile_A_Type_Twice()
        {
            Comparer.Configure<CompileTwice>()
                .Ignore(x => x.Name)
                .Compile.Now();

            Comparer.Configure<CompileTwice>()
                .Ignore(x => x.Id)
                .Compile.Now();
        }
    }
}