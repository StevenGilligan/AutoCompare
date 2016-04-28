using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoCompare.Tests
{
    [TestClass]
    public partial class BuilderTests : AutoCompareBaseTest
    {
        [TestMethod]
        public void Compile_A_Child_Type()
        {
            SutEngine.Configure<Child>()
                .Compile.Now();
        }

        [TestMethod]
        public void Compile_A_Type_With_Circular_References()
        {
            SutEngine.Configure<ParentCirularRef>()
                .Compile.Now();
        }

        [TestMethod]
        public void Compile_Async()
        {
            Assert.IsFalse(SutEngine.IsTypeConfigured(typeof(SimpleModel)));

            SutEngine.Configure<SimpleModel>()
                .Compile.Async();

            Assert.IsTrue(SutEngine.IsTypeConfigured(typeof(SimpleModel)));
            Assert.IsFalse(SutEngine.IsTypeCompiled(typeof(SimpleModel)));

            var comparer = SutEngine.Get<SimpleModel>();

            Assert.IsTrue(SutEngine.IsTypeConfigured(typeof(SimpleModel)));
            Assert.IsTrue(SutEngine.IsTypeCompiled(typeof(SimpleModel)));
        }

        [TestMethod]
        public void Compile_A_Type_With_No_Public_Properties_To_Compare()
        {
            var diff = SutEngine.Compare(new NoPublicProperty(1), new NoPublicProperty(2));
            Assert.AreEqual(0, diff.Count);
        }

        [TestMethod]
        public void Compile_A_Type_With_All_Properties_Ignored()
        {
            SutEngine.Configure<HasIgnores>()
                .Ignore(x => x.Id)
                .Ignore(x => x.IgnoreChild)
                .Ignore(x => x.IgnoreValue);
            var diff = SutEngine.Compare(new HasIgnores() { Id = 1, IgnoreValue = 100 },
                new HasIgnores() { Id = 2, IgnoreValue = 200 });
            Assert.AreEqual(0, diff.Count);
        }

        [TestMethod]
        public void Compile_A_Type_With_IEnumerable_Parent_Class()
        {
            SutEngine.Configure<InheritedIEnumerableModel>()
                .Compile.Now();
        }

        [TestMethod]
        public void Compile_A_Configured_Type_With_IEnumerable_Parent_Class()
        {
            SutEngine.Configure<InheritedIEnumerableModel>()
                .For(x => x.Children, x => x.MatchUsing(y => y.Id))
                .Compile.Now();
        }

        [TestMethod]
        public void Compile_A_Type_With_A_Struct_Member()
        {
            SutEngine.Configure<StructModel>()
                .Compile.Now();
        }
    }
}