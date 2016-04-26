using System;
using System.Collections.Generic;

namespace AutoCompare.Tests
{
    /// <summary>
    /// Single flat model with basic value properties
    /// </summary>
    public class SimpleModel
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

    public enum State
    {
        Unknown,
        Active,
        Inactive
    }

    /// <summary>
    /// A model with a nested child object
    /// </summary>
    public class NestedModel
    {
        public long Id { get; set; }
        public ChildModel Child { get; set; }
    }

    /// <summary>
    /// A nested model with another nested child object
    /// </summary>
    public class ChildModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public GrandChildModel GrandChild { get; set; }
    }

    /// <summary>
    /// Third level down
    /// </summary>
    public class GrandChildModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }

    /// <summary>
    /// A list of value properties
    /// </summary>
    public class HasList
    {
        public List<int> Ids { get; set; }
    }

    /// <summary>
    /// A model that contains a dictionary of basic value key and values
    /// </summary>
    public class HasDictionary
    {
        public Dictionary<string, string> Names { get; set; }
    }

    /// <summary>
    /// A model that is configured with ignored properties
    /// </summary>
    public class HasIgnores
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
    public class NestedList
    {
        public List<GrandChildModel> Children { get; set; }
    }

    /// <summary>
    /// A model that contains a list of complex children.
    /// The default value for the ID property must be specified
    /// so the Comparer can detect which children were added,
    /// deleted or modified
    /// </summary>
    public class NestedListWithDefault
    {
        public List<GrandChildModel> Children { get; set; }
    }

    /// <summary>
    /// A model that contains a dictionary where the Key is the ID of the child object
    /// </summary>
    public class ObjectDictionary
    {
        public Dictionary<int, GrandChildModel> Nested { get; set; }
    }

    public class Parent
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class Child : Parent
    {
        public string ChildName { get; set; }
    }

    public class ParentCirularRef
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public ChildCircularRef Child { get; set; }
    }

    public class ChildCircularRef
    {
        public string ChildName { get; set; }
        public ParentCirularRef Parent { get; set; }
    }

    public class NoPublicProperty
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

    public class ArrayModel
    {
        public int Id { get; set; }
        public GrandChildModel[] ArrayChildren { get; set; }
    }

    public class IListModel
    {
        public int Id { get; set; }
        public IList<GrandChildModel> Children { get; set; }
    }

    public class IEnumerableModel
    {
        public int Id { get; set; }
        public IEnumerable<GrandChildModel> Children { get; set; }
    }

    public class IDictionaryModel
    {
        public int Id { get; set; }
        public IDictionary<int, GrandChildModel> Children { get; set; }
    }

    public class InheritedIEnumerableModel
    {
        public int Id { get; set; }
        public IEnumerableCollectionClass Children {get;set;}
    }

    public class IEnumerableCollectionClass : List<GrandChildModel>
    {

    }

    public class PublicFieldsModel
    {
        public int Id;
        public bool Check;
        public string Name;
        public List<int> Values;
        public int Ignored;
    }
}
