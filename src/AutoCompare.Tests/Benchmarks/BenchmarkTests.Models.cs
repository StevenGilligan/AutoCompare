using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCompare.Tests.Benchmarks
{
    public partial class BenchmarkTests
    {
        /// <summary>
        /// Single flat model with basic value properties
        /// </summary>
        private class SimpleModel
        {
            public long Id { get; set; }
            public bool Check { get; set; }
            public string Name { get; set; }
            public decimal Value { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan Time { get; set; }
            public State State { get; set; }
            public bool? Nullable { get; set; }
        }

        private enum State
        {
            Unknown,
            Active,
            Inactive
        }

        /// <summary>
        /// A model with a nested child object
        /// </summary>
        private class NestedModel
        {
            public long Id { get; set; }
            public ChildModel Child { get; set; }
        }

        /// <summary>
        /// A nested model with another nested child object
        /// </summary>
        private class ChildModel
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public GrandChildModel GrandChild { get; set; }
        }

        /// <summary>
        /// Third level down
        /// </summary>
        private class GrandChildModel
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public int Value { get; set; }
        }

    }
}
