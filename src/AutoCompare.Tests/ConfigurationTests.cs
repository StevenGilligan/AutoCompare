using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

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
                .For(x => x.ArrayChildren, x => x.MatchUsing(y => y.Id));
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

        [TestMethod]
        public void Configure_IList()
        {
            SutEngine.Configure<IListModel>()
                .For(x => x.Children, x => x.MatchUsing(y => y.Id));

            var oldModel = new IListModel()
            {
                Id = 1,
                Children = new List<GrandChildModel>
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

            var newModel = new IListModel()
            {
                Id = 1,
                Children = new List<GrandChildModel>
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

        [TestMethod]
        public void Configure_IEnumerable()
        {
            SutEngine.Configure<IEnumerableModel>()
                .For(x => x.Children, x => x.MatchUsing(y => y.Id));

            var oldModel = new IEnumerableModel()
            {
                Id = 1,
                Children = new List<GrandChildModel>
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

            var newModel = new IEnumerableModel()
            {
                Id = 1,
                Children = new List<GrandChildModel>
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

        [TestMethod]
        public void Configure_IDictionary()
        {
            SutEngine.Configure<IDictionaryModel>();

            var oldModel = new IDictionaryModel()
            {
                Id = 1,
                Children = new Dictionary<int, GrandChildModel>
                {
                    {100,
                    new GrandChildModel()
                    {
                        Id = 100,
                        Name = "Name 1",
                        Value = 100
                    }},
                    {200,
                    new GrandChildModel()
                    {
                        Id = 200,
                        Name = "Name 2",
                        Value = 200
                    }}
                },
            };

            var newModel = new IDictionaryModel()
            {
                Id = 1,
                Children = new Dictionary<int, GrandChildModel>
                {
                    { 100,
                    new GrandChildModel()
                    {
                        Id = 100,
                        Name = "Name 1",
                        Value = 100
                    } },
                    {300,
                    new GrandChildModel()
                    {
                        Id = 300,
                        Name = "Name 3",
                        Value = 300
                    }}
                },
            };

            var diff = SutEngine.Compare(oldModel, newModel);
            Assert.AreEqual(6, diff.Count);
        }

        [TestMethod]
        public void Configure_A_Type_With_Only_Public_Fields()
        {
            SutEngine.Configure<PublicFieldsModel>()
                .ComparePublicFields()
                .Ignore(x => x.Ignored);

            var oldModel = new PublicFieldsModel()
            {
                Id = 1,
                Check = false,
                Name = "Name",
                Values = new List<int>() { 1, 2, 3, 4 },
                Ignored = 5,
            };

            var newModel = new PublicFieldsModel()
            {
                Id = 1,
                Check = true,
                Name = "Name 2",
                Values = new List<int>() { 2, 3, 4, 5 },
                Ignored = 6,
            };

            var diff = SutEngine.Compare(oldModel, newModel);
            Assert.AreEqual(4, diff.Count);
        }

        [TestMethod]
        public void Configure_A_Type_Ignoring_Public_Fields()
        {
            SutEngine.Configure<PublicFieldsModel>()
                .Ignore(x => x.Ignored);

            var oldModel = new PublicFieldsModel()
            {
                Id = 1,
                Check = false,
                Name = "Name",
                Values = new List<int>() { 1, 2, 3, 4 },
                Ignored = 5,
            };

            var newModel = new PublicFieldsModel()
            {
                Id = 1,
                Check = true,
                Name = "Name 2",
                Values = new List<int>() { 2, 3, 4, 5 },
                Ignored = 6,
            };

            var diff = SutEngine.Compare(oldModel, newModel);
            Assert.AreEqual(0, diff.Count);
        }
    }
}
