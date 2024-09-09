using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

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

        // Readme
        // BUG 1: FindMatchTest_Symbol_Upper_Lower test failed. Symbol LowerCase was done. Identical means to ignore Upper Case LowerCase.
        // If requiremnts is case sensitive then need to remove lower cass.
        // If requiremnts is NOT case sensitive then need to make lower cass for both
        // BUG 2: FindMatchTest_Quantity_DoesNoMatchTolerance test failed. Need to fix Math.Abs(num1 - num2); 
        // BUG 3: FindMatchTest_Quantity_MatchTolerance5_Equal test failed. Was missed ">="
        // BUG 4: Not a bug, but API Improvement suggestion:
        // string symbol to be replace by ENUM Symbol in the CreateRecord method. Similar to Side
        ///I am not sure if it is a bug or symbol can be any string required!!!!
        // CreateRecord(string symbol, int quantity, double price, DateTime settlementDate, Side side)
        // BUG 5: FindMatchTest_Symbol_NullValues - it is NullReferenceException that should be fixed in the API.
        // i do not fix it as this is not in scope i believe.  
        // BUG 6: FindMatchTest_MoreThen2IdendicalRecordsBuy - failed. 
        // Expected: This service finds matches between 2 records – each record can be matched to only one other record.
        // Actual: test returned TRue even if more then one identical 
        // BUG 7: FindMatchTest_Price_IdenticalInDecimal3dec failed
        // Price type decimal repklaced to double 
        // BUG 8: Test Case Faile. TC_1012_ Date Mismatch by Minutes,Seconds,Milliseconds, Time Zones failed
        // Helper method for creating records

      [DataTestMethod]
      [DynamicData(nameof(GetTestCases), DynamicDataSourceType.Method)]
      public void FindMatchTestFromFile(
      string symbol1, int quantity1, double price1, string dateString1, Side side1,
      string symbol2, int quantity2, double price2, string dateString2, Side side2,
      bool expectedMatch, string displayName)
        {
            // Parse date strings to DateTime
            var date1 = DateTime.Parse(dateString1);
            var date2 = DateTime.Parse(dateString2);

            // Create records for both sides
            var record1 = CreateRecord(symbol1, quantity1, price1, date1, side1);
            var record2 = CreateRecord(symbol2, quantity2, price2, date2, side2);

            // Check if records match
            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            // Assert if the result matches the expected outcome
            Assert.AreEqual(expectedMatch, isMatched, displayName);
        }

        // Helper method for creating records
        private Record CreateRecord(string symbol, int quantity, double price, DateTime date, Side side)
        {
            return _matchingApi.CreateRecord((Symbol)Enum.Parse(typeof(Symbol), symbol), quantity, price, date, side);
        }
        public static IEnumerable<object[]> GetTestCases()
        {
            var filePath = "C:\\logs\\testdataMy.csv"; // Path to your test data file
            var lines = File.ReadAllLines(filePath);
            var testCases = new List<object[]>();

            try
            {
                foreach (var line in lines.Skip(1)) // Skip header line
                {
                    var columns = line.Split(',');
                    var testCase = new object[]
                    {
                        columns[0], // Symbol1
                        int.Parse(columns[1]), // Quantity1
                        double.Parse(columns[2]), // Price1
                        columns[3], // Date1
                        Enum.Parse<Side>(columns[4]), // Side1
                        columns[5], // Symbol2
                        int.Parse(columns[6]), // Quantity2
                        double.Parse(columns[7]), // Price2
                        columns[8], // Date2
                        Enum.Parse<Side>(columns[9]), // Side2
                        bool.Parse(columns[10]), // ExpectedMatch
                        columns[11] // DisplayName
                    };

                    testCases.Add(testCase);
                }
            }
            catch (Exception ex)
            {
                return testCases;
            }

            return testCases;
        }
        public void FindMatchTest(string symbol1, string symbol2, int quantity1, int quantity2, int price, string date, Side side1, Side side2, bool expectedMatch)
        {
            var dateValue = DateTime.Parse(date);

            var record1 = CreateRecord(symbol1, quantity1, price, dateValue, side1);
            var record2 = CreateRecord(symbol2, quantity2, price, dateValue, side2);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.AreEqual(expectedMatch, isMatched);
        }
         
        // Test case to ensure each record can only match one other record
        [TestMethod]
        public void TC_1020_FindMatchTest_OnlyOneMatchAllowed()
        {
            var date = DateTime.Parse("2023-05-03");

            // Create three records
            var record1 = CreateRecord("GOOG", 1000, 1250, date, Side.Buy);
            var record2 = CreateRecord("GOOG", 1000, 1250, date, Side.Sell); // Should match with record1
            
            // Record 1 matches with Record 2
            Assert.IsTrue(_matchingApi.CheckIfRecordsMatched(record1, record2));

            var record3 = CreateRecord("GOOG", 1000, 1250, date, Side.Sell); // Should not match because record1 is already matched to record2

            // Record 1 should NOT match with Record 3 because it can only match with one other record
            Assert.IsFalse(_matchingApi.CheckIfRecordsMatched(record1, record3));

            // Record 3 should NOT match with Record 2 either, as Record 2 is already matched to Record 1
            Assert.IsFalse(_matchingApi.CheckIfRecordsMatched(record2, record3));
        }

        [DataRow( DisplayName = "TC_1014_FindMatchTest_TwoDistinctPairs")]
        [TestMethod]
        public void TC_1019_FindMatchTest_TwoDistinctPairs()
        {
            var date = DateTime.Parse("2023-05-03");

            // Create four records, two distinct pairs
            var record1 = CreateRecord("GOOG", 1000, 1250, date, Side.Buy);
            var record2 = CreateRecord("GOOG", 1000, 1250, date, Side.Sell); // Should match with record1
            var record3 = CreateRecord("AAPL", 1000, 1250, date, Side.Buy);
            var record4 = CreateRecord("AAPL", 1000, 1250, date, Side.Sell); // Should match with record3

            // Test that correct pairs match
            Assert.IsTrue(_matchingApi.CheckIfRecordsMatched(record1, record2)); // Google pair
            Assert.IsTrue(_matchingApi.CheckIfRecordsMatched(record3, record4)); // Apple pair

            // Ensure no incorrect matches
            Assert.IsFalse(_matchingApi.CheckIfRecordsMatched(record1, record3));
            Assert.IsFalse(_matchingApi.CheckIfRecordsMatched(record2, record4));
        }

        /// <summary>
        /// //////////////////////////////////////////////////////////
        /// </summary>
    }
}