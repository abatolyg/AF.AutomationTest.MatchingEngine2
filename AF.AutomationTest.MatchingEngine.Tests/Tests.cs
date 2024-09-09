using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace AF.AutomationTest.MatchingEngine.Tests
{
    [TestClass]
    public class Tests
    {
        private static MatchingApi _matchingApi;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            _matchingApi = new MatchingApi();
        }
        
        [TestInitialize]
        public void TestInitialize()
        {
            _matchingApi.ClearData();
        }

        // example test
        [TestMethod]
        public void FindMatchTest()
        {
            var date = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord("Test", 130, 10, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord("test", 150, 10, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsTrue(isMatched);
        }

    }
}