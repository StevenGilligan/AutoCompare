using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace AutoCompare.Tests
{
    [TestClass]
    public partial class BuilderTests
    {
        [TestMethod]
        public void Compile_A_Child_Type()
        {
            Comparer.Configure<Child>()
                .Compile.Now();
        }

        [TestMethod]
        public void Compile_A_Type_With_Circular_References()
        {
            Comparer.Configure<ParentCirularRef>()
                .Compile.Now();
        }
    }
}
