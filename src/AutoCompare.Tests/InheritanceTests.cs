using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoCompare.Tests
{
    [TestClass]
    public partial class InheritanceTests
    {
        [TestMethod]
        public void When_I_Compile_A_Child_Type_It_Works()
        {
            var comparer = Comparer.GetComparer<Child>();
        }

        [TestMethod]
        public void When_I_Compile_A_Type_With_Circular_Ref_It_Works()
        {
            var comparer = Comparer.GetComparer<ParentCirularRef>();
            var objA = new ParentCirularRef()
            {
                Id = 1,
                Name = "Parent",
                Child = new ChildCircularRef()
                {
                    ChildName = "Child"
                }
            };

            objA.Child.Parent = objA;
            var objB = new ParentCirularRef()
            {
                Id = 2,
                Name = "Parent 2",
                Child = new ChildCircularRef()
                {
                    ChildName = "Child 2"
                }
            };
            objB.Child.Parent = objB;

            var updates = comparer(objA, objB);
        }
    }
}
