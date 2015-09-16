using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCompare.Tests
{
    [TestClass]
    public partial class ComparerTests
    {
        [TestMethod]
        public void When_I_Compare_Two_Objects_I_Get_A_List_Of_Updated_Properties()
        {
            var comparer = Comparer.GetComparer<SimpleModel>();
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

            var changes = comparer(oldModel, newModel);
            Assert.AreEqual(changes.Count(), 7);
            Assert.IsNotNull(changes.Single(x => x.Name == "Check" && (bool)x.OldValue == true && (bool)x.NewValue == false));
            Assert.IsNotNull(changes.Single(x => x.Name == "Name" && (string)x.OldValue == "Name" && (string)x.NewValue == "Name?"));
            Assert.IsNotNull(changes.Single(x => x.Name == "Value" && (decimal)x.OldValue == 1.23m && (decimal)x.NewValue == 10.23m));
            Assert.IsNotNull(changes.Single(x => x.Name == "Date" && (DateTime)x.OldValue != (DateTime)x.NewValue));
            Assert.IsNotNull(changes.Single(x => x.Name == "Time" && (TimeSpan)x.OldValue != (TimeSpan)x.NewValue));
            Assert.IsNotNull(changes.Single(x => x.Name == "State" && (State)x.OldValue != (State)x.NewValue));
            Assert.IsNotNull(changes.Single(x => x.Name == "Nullable" && (bool?)x.OldValue == null && (bool?)x.NewValue == true));
        }

        [TestMethod]
        public void When_I_Compare_Nested_Objects_I_Get_Nested_Properties()
        {
            var comparer = Comparer.GetComparer<NestedModel>();
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

            var changes = comparer(oldModel, newModel);
            Assert.AreEqual(changes.Count(), 4);
            Assert.IsNotNull(changes.Single(x => x.Name == "Child.Name" && (string)x.OldValue == "Child" && (string)x.NewValue == "Child 2"));
            Assert.IsNotNull(changes.Single(x => x.Name == "Child.GrandChild.Name" && (string)x.OldValue == "GrandChild" && (string)x.NewValue == "GrandChild 2"));
            Assert.IsNotNull(changes.Single(x => x.Name == "Child.GrandChild.Id" && (long)x.OldValue == 3 && (long)x.NewValue == 4));
            Assert.IsNotNull(changes.Single(x => x.Name == "Child.GrandChild.Value" && (int)x.OldValue == 100 && (int)x.NewValue == 200));
        }

        [TestMethod]
        public void When_I_Compare_IEnumerable_I_Get_Added_And_Removed_Values()
        {
            var comparer = Comparer.GetComparer<HasList>();
            var oldModel = new HasList
            {
                Ids = new List<int> { 1, 2, 3, 4, 5 }
            };
            var newModel = new HasList
            {
                Ids = new List<int> { 1, 3, 4, 5, 6 }
            };
            var changes = comparer(oldModel, newModel);
            Assert.AreEqual(changes.Count(), 2);
            Assert.IsNotNull(changes.Single(x => x.Name == "Ids" && x.OldValue is int && (int)x.OldValue == 2));
            Assert.IsNotNull(changes.Single(x => x.Name == "Ids" && x.NewValue is int && (int)x.NewValue == 6));
        }

        [TestMethod]
        public void When_I_Compare_IDictionary_I_Get_Updated_Added_And_Removed_Values()
        {
            var comparer = Comparer.GetComparer<HasDictionary>();
            var oldModel = new HasDictionary
            {
                Names = new Dictionary<string, string>
                {
                    {"fr", "Salut"},
                    {"en", "Hi"}
                }
            };
            var newModel = new HasDictionary
            {
                Names = new Dictionary<string, string>
                {
                    {"en", "Hello"},
                    {"es", "Hola"}
                }
            };

            var changes = comparer(oldModel, newModel);
            Assert.AreEqual(changes.Count(), 3);
            Assert.IsNotNull(changes.Single(x => x.Name == "Names.fr" && (string)x.OldValue == "Salut"));
            Assert.IsNotNull(changes.Single(x => x.Name == "Names.es" && (string)x.NewValue == "Hola"));
            Assert.IsNotNull(changes.Single(x => x.Name == "Names.en" && (string)x.OldValue == "Hi" && (string)x.NewValue == "Hello"));
        }

        [TestMethod]
        public void When_I_Ignore_A_Property_It_Should_Not_Test_It()
        {
            Comparer.Configure<HasIgnores>()
                .Ignore(x => x.IgnoreChild)
                .Ignore(x => x.IgnoreValue);

            var oldModel = new HasIgnores
            {
                Id = 1,
                IgnoreValue = 5,
                IgnoreChild = new GrandChildModel
                {
                    Id = 10,
                    Name = "A",
                    Value = 100
                }
            };
            var newModel = new HasIgnores
            {
                Id = 2,
                IgnoreValue = 55,
                IgnoreChild = new GrandChildModel
                {
                    Id = 20,
                    Name = "B",
                    Value = 200
                }
            };

            var changes = Comparer.Compare(oldModel, newModel);
            Assert.AreEqual(changes.Count(), 1);
            Assert.IsNotNull(changes.Single(x => x.Name == "Id" && (long)x.OldValue == 1 && (long)x.NewValue == 2));
        }

        [TestMethod]
        public void When_I_Compare_Null_Objects_I_Get_All_Properties()
        {
            var model = new SimpleModel
            {
                Id = 1,
                Check = true,
                Name = "Name",
                Value = 1.23m,
                Date = new DateTime(2015, 01, 01, 12, 50, 30),
                Time = new TimeSpan(5, 4, 3)
            };

            var nullToModel = Comparer.Compare(null, model);
            var modelToNull = Comparer.Compare(model, null);
            Assert.IsTrue(nullToModel.All(x =>
                modelToNull.Single(y => x.Name == y.Name && object.Equals(y.OldValue, x.NewValue)) != null));
        }

        [TestMethod]
        public void When_I_Compare_Nested_Null_Objects_It_Works()
        {
            var oldModel = new NestedModel()
            {
                Id = 10,
                Child = null
            };

            var newModel = new NestedModel()
            {
                Id = 10,
                Child = new ChildModel()
                {
                    Id = 100,
                    Name = "Child",
                    GrandChild = new GrandChildModel()
                    {
                        Id = 1000,
                        Name = "GrandChild",
                        Value = 500,
                    }
                }
            };

            var changes = Comparer.Compare(oldModel, newModel);
            Assert.AreEqual(changes.Count(), 5);
            Assert.IsNotNull(changes.Single(x => x.Name == "Child.Id"));
            Assert.IsNotNull(changes.Single(x => x.Name == "Child.GrandChild.Value"));
            Assert.IsNotNull(changes.Single(x => x.Name == "Child.GrandChild.Name"));
        }

        [TestMethod]
        public void When_I_Compare_Dictionary_Of_Objects_It_Does_A_Deep_Comparison()
        {
            Comparer.Configure<ObjectDictionary>()
                .DeepCompare(x => x.Nested);

            var oldModel = new ObjectDictionary()
            {
                Nested = new Dictionary<int, GrandChildModel>()
                {
                    {1, new GrandChildModel()
                    {
                        Id = 100,
                        Name = "Name 1",
                        Value = 100
                    }},
                    {2, new GrandChildModel()
                    {
                        Id = 200,
                        Name = "Name 2",
                        Value = 200
                    }},
                }
            };

            var newModel = new ObjectDictionary()
            {
                Nested = new Dictionary<int, GrandChildModel>()
                {
                    {1, new GrandChildModel()
                    {
                        Id = 100,
                        Name = "Name 1 - Changed",
                        Value = 150
                    }},
                    {3, new GrandChildModel()
                    {
                        Id = 300,
                        Name = "Name 3",
                        Value = 300
                    }},
                }
            };

            var changes = Comparer.Compare(oldModel, newModel);
            Assert.AreEqual(changes.Count(), 8);
            Assert.IsNotNull(changes.Single(x => x.Name == "Nested.1.Value"));
        }

        [TestMethod]
        public void When_I_Compare_Lists_Of_Objects_I_Can_Specify_An_Id()
        {
            Comparer.Configure<NestedList>()
                .Enumerable(x => x.Children, x => x.DeepCompare(y => y.Id));

            var oldModel = new NestedList()
            {
                Children = new List<GrandChildModel>()
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
                    },
                }
            };

            var newModel = new NestedList()
            {
                Children = new List<GrandChildModel>()
                {
                    new GrandChildModel()
                    {
                        Id = 100,
                        Name = "Name 1 - Changed",
                        Value = 150
                    },
                    new GrandChildModel()
                    {
                        Id = 300,
                        Name = "Name 3",
                        Value = 300
                    },
                    new GrandChildModel()
                    {
                        Id = 400,
                        Name = "Name 4",
                        Value = 200
                    }
                }
            };

            var changes = Comparer.Compare(oldModel, newModel);
            Assert.AreEqual(changes.Count(), 11);
            Assert.IsNotNull(changes.Single(x => x.Name == "Children.100.Name"));
        }

        [TestMethod]
        public void When_I_Compare_Lists_Of_Objects_I_Can_Specify_A_Default_Key()
        {
            Comparer.Configure<NestedListWithDefault>()
                .Enumerable(x => x.Children, x => x.DeepCompare(y => y.Id, 0));

            var oldModel = new NestedListWithDefault()
            {
                Children = new List<GrandChildModel>()
                {
                    new GrandChildModel()
                    {
                        Id = 100,
                        Name = "Name 1",
                        Value = 100
                    },
                }
            };

            var newModel = new NestedListWithDefault()
            {
                Children = new List<GrandChildModel>()
                {
                    new GrandChildModel()
                    {
                        Id = 100,
                        Name = "Name 1",
                        Value = 100
                    },
                    new GrandChildModel()
                    {
                        Id = 0,
                        Name = "Name 2",
                        Value = 200
                    },
                    new GrandChildModel()
                    {
                        Id = 0,
                        Name = "Name 3",
                        Value = 300
                    },
                    new GrandChildModel()
                    {
                        Id = 0,
                        Name = "Name 4",
                        Value = 400
                    },
                }
            };

            var changes = Comparer.Compare(oldModel, newModel);
            Assert.AreEqual(changes.Count(), 9);
            Assert.IsNotNull(changes.Count(x => x.Name == "Children.0.Id" && (long)x.NewValue == 0) == 3);
        }

        [TestMethod]
        public void When_Models_Are_Null_Do_Not_Explode()
        {
            var changes = Comparer.Compare<NestedModel>(null, null);
            Assert.IsFalse(changes.Any());
        }
    }
}