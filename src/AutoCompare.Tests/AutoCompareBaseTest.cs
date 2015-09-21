using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoCompare.Tests
{
    public class AutoCompareBaseTest
    {
        protected IComparerEngine SutEngine
        {
            get; private set;
        }

        [TestInitialize]
        public void InitializeSut()
        {
            SutEngine = new Engine();
        }
    }
}