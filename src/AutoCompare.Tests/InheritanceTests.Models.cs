using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCompare.Tests
{
    public partial class InheritanceTests
    {
        private class Parent
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        private class Child : Parent
        {
            public string ChildName { get; set; }
        }

        private class ParentCirularRef
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public ChildCircularRef Child { get; set; }
        }

		private class ChildCircularRef
        {
			public string ChildName { get; set; }
			public ParentCirularRef Parent { get; set; }
        }
    }
}
