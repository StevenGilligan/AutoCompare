using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCompare.Tests
{
    /// <summary>
    /// Single flat model with basic value properties
    /// </summary>
    internal class SimpleModel
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

    internal enum State
    {
        Unknown,
        Active,
        Inactive
    }

    /// <summary>
    /// A model with a nested child object
    /// </summary>
    internal class NestedModel
    {
        public long Id { get; set; }
        public ChildModel Child { get; set; }
    }

    /// <summary>
    /// A nested model with another nested child object
    /// </summary>
    internal class ChildModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public GrandChildModel GrandChild { get; set; }
    }

    /// <summary>
    /// Third level down
    /// </summary>
    internal class GrandChildModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }

    /// <summary>
    /// A list of value properties
    /// </summary>
    internal class HasList
    {
        public List<int> Ids { get; set; }
    }

    /// <summary>
    /// A model that contains a dictionary of basic value key and values
    /// </summary>
    internal class HasDictionary
    {
        public Dictionary<string, string> Names { get; set; }
    }

    /// <summary>
    /// A model that is configured with ignored properties
    /// </summary>
    internal class HasIgnores
    {
        public long Id { get; set; }
        public int IgnoreValue { get; set; }
        public GrandChildModel IgnoreChild { get; set; }
    }

    /// <summary>
    /// A model that contains a list of complex children.
    /// Proper configuration is required to compare nested items.
    /// An ID property must be configured to be able to detect when the same child
    /// object is modified
    /// </summary>
    internal class NestedList
    {
        public List<GrandChildModel> Children { get; set; }
    }

    /// <summary>
    /// A model that contains a list of complex children.
    /// The default value for the ID property must be specified
    /// so the Comparer can detect which children were added,
    /// deleted or modified
    /// </summary>
    internal class NestedListWithDefault
    {
        public List<GrandChildModel> Children { get; set; }
    }

    /// <summary>
    /// A model that contains a dictionary where the Key is the ID of the child object
    /// </summary>
    internal class ObjectDictionary
    {
        public Dictionary<int, GrandChildModel> Nested { get; set; }
    }
}
