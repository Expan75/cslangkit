using CSLangKit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSLangKit.UnitTests
{
    [TestClass]
    public class ExampleTests
    {
        [TestMethod]
        public void ReturnTrueReturnsTrue()
        {
            var result = Example.ReturnTrue();
            Assert.IsTrue(result);
        }
    }
}
