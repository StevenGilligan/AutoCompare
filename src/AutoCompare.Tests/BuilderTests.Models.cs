using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCompare.Tests
{
    public partial class BuilderTests
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
        
        private class NoPublicProperty
        {
            private int Id { get; set; }
            private bool _bool;
            public bool DoSomething() => !_bool;

            public NoPublicProperty(int id)
            {
                Id = id;
                _bool = Id % 2 == 0;
            }
        }

        private class IgnoreAllProperties
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class CompileTwice
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
