using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using com.PorcupineSupernova.RootCauseTreeCore;

namespace com.PorcupineSupernova.RootCauseTreeTests
{
    [TestClass]
    public class SequentialGuidTests
    {
        [TestMethod]
        public void TestSequentiality()
        {
            Guid[] guids = new Guid[100];
            for (int i = 0; i < 100; i++)
            {
                guids[i] = SequentialGuid.NewGuid();
                System.Threading.Thread.Sleep(10);  //DateTime.Now's resolution is too low to not use a sleep command.
            }
            for (int i = 1; i < 100; i++)
            {
                Assert.AreEqual(1,guids[i].CompareTo(guids[i - 1]),$"Failed on {i}");
            }
        }
    }
}
