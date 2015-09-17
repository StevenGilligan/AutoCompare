using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace AutoCompare.Tests.Benchmarks
{
    [TestClass]
    public partial class BenchmarkTests
    {
        [TestCategory("Benchmarks")]
        [TestMethod]
        public void Benchmark_Simple_Model()
        {
            var oldModel = new SimpleModel
            {
                Id = 1,
                Check = true,
                Name = "Name",
                Value = 1.23m,
                Date = new DateTime(2015, 01, 01, 12, 50, 30),
                Time = new TimeSpan(5, 4, 3),
                State = State.Inactive,
                Nullable = null,
            };

            var newModel = new SimpleModel
            {
                Id = 1,
                Check = false,
                Name = "Name?",
                Value = 10.23m,
                Date = new DateTime(2015, 01, 02, 12, 50, 30),
                Time = new TimeSpan(5, 4, 1),
                State = State.Active,
                Nullable = true,
            };

            var sw = new Stopwatch();
            sw.Start();
            var comparer = Comparer.Get<SimpleModel>();
            sw.Stop();
            var compilation = sw.ElapsedMilliseconds;
            sw.Reset();

            sw.Start();
            var numIterations = 100000;
            for (var i = 0; i < numIterations; i++)
            {
                var updates = comparer(oldModel, newModel);
            }
            sw.Stop();

            var benchmark = sw.ElapsedMilliseconds;
            Console.WriteLine($"Compilation took {compilation} ms.");
            Console.WriteLine($"{numIterations} iterations took {benchmark} ms.");
            Assert.IsTrue(benchmark < 500);
        }

        [TestCategory("Benchmarks")]
        [TestMethod]
        public void Benchmark_Nested_Model()
        {
            var oldModel = new NestedModel
            {
                Id = 1,
                Child = new ChildModel
                {
                    Id = 2,
                    Name = "Child",
                    GrandChild = new GrandChildModel
                    {
                        Id = 3,
                        Name = "GrandChild",
                        Value = 100,
                    }
                }
            };
            var newModel = new NestedModel
            {
                Id = 1,
                Child = new ChildModel
                {
                    Id = 2,
                    Name = "Child 2",
                    GrandChild = new GrandChildModel
                    {
                        Id = 4,
                        Name = "GrandChild 2",
                        Value = 200,
                    }
                }
            };

            var sw = new Stopwatch();

            sw.Start();
            var comparer = Comparer.Get<NestedModel>();
            sw.Stop();
            var compilation = sw.ElapsedMilliseconds;
            sw.Reset();

            sw.Start();
            var numIterations = 100000;
            for (var i = 0; i < numIterations; i++)
            {
                var updates = comparer(oldModel, newModel);
            }
            sw.Stop();

            var benchmark = sw.ElapsedMilliseconds;
            Console.WriteLine($"Compilation took {compilation} ms.");
            Console.WriteLine($"{numIterations} iterations took {benchmark} ms.");
            Assert.IsTrue(benchmark < 500);
        }
    }
}
