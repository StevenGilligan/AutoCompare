using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace AutoCompare.Tests
{
    [TestClass]
    public class ComparerTests
    {
        [TestMethod]
        public void Static_Comparer_Compare_A_Simple_Type()
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

            var changes = Comparer.Compare(oldModel, newModel);
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
        public void Static_Comparer_Configure_A_Type()
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

            var comparer = Comparer.Get<HasIgnores>();
            var changes = comparer(oldModel, newModel);
            Assert.AreEqual(changes.Count(), 1);
            Assert.IsNotNull(changes.Single(x => x.Name == "Id" && (long)x.OldValue == 1 && (long)x.NewValue == 2));
        }

        [TestMethod]
        public void Static_Comparer_A_Value_Type()
        {
            var oldModel = new StructModel
            {
                Id = 1,
                StructMember = new StructMember
                {
                    Id = 1,
                    Value = "Old"
                }
            };

            var newModel = new StructModel
            {
                Id = 1,
                StructMember = new StructMember
                {
                    Id = 1,
                    Value = "New"
                }
            };

            var changes = Comparer.Compare(oldModel, newModel);
            Assert.AreEqual(changes.Count(), 1);
            Assert.IsNotNull(changes.Single(x => x.Name == "StructMember.Value" && (string)x.OldValue == "Old" && (string)x.NewValue == "New"));
        }

	    [TestMethod]
	    public void Static_Comparer_Compare_A_Simple_Type_With_Ignored_Type()
	    {
		    var oldModel = new SimpleModelIgnore
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

		    var newModel = new SimpleModelIgnore
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

		    Comparer.Configure<SimpleModelIgnore>().IgnoreType(typeof(DateTime));

		    var changes = Comparer.Compare(oldModel, newModel);
		    Assert.AreEqual(changes.Count(), 6);
		    Assert.IsNotNull(changes.Single(x => x.Name == "Check" && (bool)x.OldValue == true && (bool)x.NewValue == false));
		    Assert.IsNotNull(changes.Single(x => x.Name == "Name" && (string)x.OldValue == "Name" && (string)x.NewValue == "Name?"));
		    Assert.IsNotNull(changes.Single(x => x.Name == "Value" && (decimal)x.OldValue == 1.23m && (decimal)x.NewValue == 10.23m));
		    Assert.IsNotNull(changes.Single(x => x.Name == "Time" && (TimeSpan)x.OldValue != (TimeSpan)x.NewValue));
		    Assert.IsNotNull(changes.Single(x => x.Name == "State" && (State)x.OldValue != (State)x.NewValue));
		    Assert.IsNotNull(changes.Single(x => x.Name == "Nullable" && (bool?)x.OldValue == null && (bool?)x.NewValue == true));
	    }
	}
}
