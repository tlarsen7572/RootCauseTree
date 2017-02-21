using Microsoft.VisualStudio.TestTools.UnitTesting;
using com.PorcupineSupernova.RootCauseTreeCore;

namespace com.PorcupineSupernova.RootCauseTreeTests
{
    [TestClass]
    public class SequentialIdTests
    {
        [TestMethod]
        public void TestSequentiality()
        {
            long[] ids = new long[100];
            for (int i = 0; i < 100; i++)
            {
                ids[i] = SequentialId.NewId();
                System.Diagnostics.Debug.WriteLine(ids[i]);
            }
            for (int i = 1; i < 100; i++)
            {
                Assert.AreEqual(1,ids[i].CompareTo(ids[i - 1]),$"Failed on {i}");
            }
        }
    }
}
