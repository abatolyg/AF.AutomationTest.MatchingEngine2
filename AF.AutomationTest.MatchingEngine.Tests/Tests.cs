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

        public void FindMatchTest(string symbol1, string symbol2, int quantity1, int quantity2, int price, string date, Side side1, Side side2, bool expectedMatch)
        {
            var dateValue = DateTime.Parse(date);

            var record1 = CreateRecord(symbol1, quantity1, price, dateValue, side1);
            var record2 = CreateRecord(symbol2, quantity2, price, dateValue, side2);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.AreEqual(expectedMatch, isMatched);
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

        [DataTestMethod]
        // Test cases for matching and non-matching conditions
        // Exact match
        [DataRow("GOOG", 1000, 1250.00, "2023-05-03", Side.Buy,
                 "GOOG", 1000, 1250.00, "2023-05-03", Side.Sell, true, DisplayName = "TC_1000_Exact Match")]
        // Symbol  
        [DataRow("GOOG", 1000, 1250.00, "2023-05-03", Side.Buy,
                 "AAPL", 1000, 1250.00, "2023-05-03", Side.Sell, false, DisplayName = "TC_1001_Symbol Mismatch. Simple")]
        [DataRow("GOOG", 1000, 1250.00, "2023-05-03", Side.Buy,
                 "VOD", 1000, 1250.00, "2023-05-03", Side.Sell, false, DisplayName = "TC_1002_Symbol Mismatch. More symbols")]
        [DataRow("GOOG", 1000, 1250.00, "2023-05-03", Side.Buy,
                 "goog", 1000, 1250.00, "2023-05-03", Side.Sell, false, DisplayName = "TC_1003_Symbol Mismatch Case Sensitive all")]
        [DataRow("Goog", 1000, 1250.00, "2023-05-03", Side.Buy,
                 "goog", 1000, 1250.00, "2023-05-03", Side.Sell, false, DisplayName = "TC_1004_Symbol Mismatch Case Sensitive one")]
        [DataRow("", 1000, 1250.00, "2023-05-03", Side.Buy,
                 "", 1000, 1250.00, "2023-05-03", Side.Sell, false, DisplayName = "TC_1004_Symbol Empty value")]
        // Quantity
        [DataRow("GOOG", 1000, 1250.00, "2023-05-03", Side.Buy,
                 "GOOG", 1010, 1250.00, "2023-05-03", Side.Sell, true, DisplayName = "TC_1005_Quantity Mismatch Within Tolerance")]
        [DataRow("GOOG", 1010, 1250.00, "2023-05-03", Side.Buy,
                 "GOOG", 1011, 1250.00, "2023-05-03", Side.Sell, true, DisplayName = "TC_1005_Quantity Mismatch Within Tolerance In another Order of records passed!")]
        [DataRow("GOOG", 1000, 1250.00, "2023-05-03", Side.Buy,
                 "GOOG", 1011, 1250.00, "2023-05-03", Side.Sell, false, DisplayName = "TC_1006_Quantity Mismatch Beyond Tolerance")]
        [DataRow("GOOG", 1011, 1250.00, "2023-05-03", Side.Buy,
                 "GOOG", 1000, 1250.00, "2023-05-03", Side.Sell, false, DisplayName = "TC_1007_Quantity Mismatch Tolerance. In another Order of records passed!")]
        // Price
        [DataRow("GOOG", 1000, 1250.00, "2023-05-03", Side.Buy,
                 "GOOG", 1000, 1251.00, "2023-05-03", Side.Sell, false, DisplayName = "TC_1008_Price Mismatch by Cents")]
        [DataRow("GOOG", 1000, 1250.1, "2023-05-03", Side.Buy,
                 "GOOG", 1000, 1250.0, "2023-05-03", Side.Sell, false, DisplayName = "TC_1009_Price Mismatch by Cents 1")]
        [DataRow("GOOG", 1000, 1250.10, "2023-05-03", Side.Buy,
                 "GOOG", 1000, 1250.11, "2023-05-03", Side.Sell, false, DisplayName = "TC_1010_Price Mismatch by Cents 2")]
        [DataRow("GOOG", 1000, 1250.111, "2023-05-03", Side.Buy,
                 "GOOG", 1000, 1250.110, "2023-05-03", Side.Sell, false, DisplayName = "TC_1011_Price Mismatch by Cents 3")]
        // Date
        [DataRow("GOOG", 1000, 1250.00, "2024-05-03T10:00:00", Side.Buy,
                 "GOOG", 1000, 1250.00, "2023-05-03T10:00:00", Side.Sell, false, DisplayName = "TC_1012_Date Mismatch in Year")]
        [DataRow("GOOG", 1000, 1250.00, "2023-05-03T10:00:00", Side.Buy,
                 "GOOG", 1000, 1250.00, "2023-06-03T10:00:00", Side.Sell, false, DisplayName = "TC_1013_Date Mismatch in Month")]
        [DataRow("GOOG", 1000, 1250.00, "2023-05-03T10:00:00", Side.Buy,
                 "GOOG", 1000, 1250.00, "2023-05-03T11:06:00", Side.Sell, false, DisplayName = "TC_1014_Date Mismatch by Minutes")]
        [DataRow("GOOG", 1000, 1250.00, "2023-05-03T11:00:00", Side.Buy,
                 "GOOG", 1000, 1250.00, "2023-05-03T11:00:10", Side.Sell, false, DisplayName = "TC_1015_Date Mismatch by Seconds")]
        [DataRow("GOOG", 1000, 1250.00, "2023-05-03T10:00:00.000", Side.Buy,
                 "GOOG", 1000, 1250.00, "2023-05-03T10:00:00.001", Side.Sell, false, DisplayName = "TC_1016_Date Mismatch by Milliseconds")]
        [DataRow("GOOG", 1000, 1250.00, "2023-05-03T10:00:00+02:00", Side.Buy,
                 "GOOG", 1000, 1250.00, "2023-05-03T10:00:00-05:00", Side.Sell, false, DisplayName = "TC_1017_Date Mismatch by Time Zones")]
        // Multiple Fields Mismatch
        [DataRow("GOOG", 1000, 1250.00, "2023-05-03", Side.Buy,
                 "AAPL", 1005, 1251.00, "2023-05-04", Side.Sell, false, DisplayName = "TC_1018_Multiple Fields Mismatch")]

        public void FindMatchTest(
            string symbol1, int quantity1, double price1, string dateString1, Side side1,
            string symbol2, int quantity2, double price2, string dateString2, Side side2,
            bool expectedMatch)
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

        [TestMethod]
        public void FindMatchTest_MultipleRecords_OnePair()
        {
            var date = DateTime.Parse("2023-05-03");

            // Create three records
            var record1 = CreateRecord("GOOG", 1000, 1250, date, Side.Buy);
            var record2 = CreateRecord("GOOG", 1000, 1250, date, Side.Sell);

            // Test matching: Record 1 should match with Record 2, but not with Record 3
            Assert.IsTrue(_matchingApi.CheckIfRecordsMatched(record1, record2));  // Should match

            var record3 = CreateRecord("GOOG", 1000, 1250, date, Side.Sell);

            Assert.IsFalse(_matchingApi.CheckIfRecordsMatched(record1, record3)); // Should not match (already matched to record2)
        }

        [TestMethod]
        public void FindMatchTest_MultipleRecords_TwoPairs()
        {
            var date = DateTime.Parse("2023-05-03");

            // Create four records
            var record1 = CreateRecord("GOOG", 1000, 1250, date, Side.Buy);
            var record2 = CreateRecord("AAPL", 1000, 1250, date, Side.Buy);
            var record3 = CreateRecord("GOOG", 1000, 1250, date, Side.Sell);
            var record4 = CreateRecord("AAPL", 1000, 1250, date, Side.Sell);

            // Test matching: Record 1 should match with Record 3, and Record 2 should match with Record 4
            Assert.IsTrue(_matchingApi.CheckIfRecordsMatched(record1, record3)); // Google pair
            Assert.IsTrue(_matchingApi.CheckIfRecordsMatched(record2, record4)); // Apple pair

            // Ensure records don't match incorrectly
            Assert.IsFalse(_matchingApi.CheckIfRecordsMatched(record1, record2)); // Should not match
            Assert.IsFalse(_matchingApi.CheckIfRecordsMatched(record3, record4)); // Should not match
        }
           
        [TestMethod]
        //[DataRow(-1)]   TODO - Use data row
        //[DataRow(0)]
        //[DataRow(2)]
        public void FindMatchTest_PositiveFullMatch()
        {
            var date = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 130, 10, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 130, 10, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsTrue(isMatched);
        }

        [TestMethod]
        public void FindMatchTest_Symbol_DoesNoMatch()
        {
            var date = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 130, 10, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.AAPL, 130, 10, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsFalse(isMatched);
        }

        [TestMethod]
        public void FindMatchTest_Quantity_DoesNoMatchTolerance()
        {
            var date = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 141, 10, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 130, 10, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsFalse(isMatched);
        }

        [TestMethod]
        public void FindMatchTest_Quantity_MatchTolerance5()
        {
            var date = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 150, 10, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsTrue(isMatched);
        }

        [TestMethod]
        public void FindMatchTest_Quantity_MatchTolerance5_AnotherOrder()
        {
            var date = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 150, 10, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsTrue(isMatched);
        }
        [TestMethod]
        public void FindMatchTest_Quantity_MatchTolerance5_Equal()
        {
            var date = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 145, 10, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 150, 10, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsTrue(isMatched);
        }
        [TestMethod]
        public void FindMatchTest_Price_NotIdentical()
        {
            var date = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 100, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 150, 10, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsFalse(isMatched);
        }

        [TestMethod]
        public void FindMatchTest_Price_NotIdenticalInDecimal()
        {
            var date = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 150, 100.5, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 150, 100, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsFalse(isMatched);
        }

        [TestMethod]
        public void FindMatchTest_Price_IdenticalInDecimal()
        {
            var date = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 150, 100.5, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 150, 100.5, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsTrue(isMatched);
        }

        [TestMethod]
        public void FindMatchTest_Price_IdenticalInDecimal2dec()
        {
            var date = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 150, 100.51, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 150, 100.52, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsFalse(isMatched);
        }


        [TestMethod]
        public void FindMatchTest_Price_IdenticalInDecimal3dec()
        {
            var date = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 150, 100.511, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 150, 100.524, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsFalse(isMatched);
        }

        [TestMethod]
        public void FindMatchTest_Type_TheSameBay()
        {
            var date = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Buy);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsFalse(isMatched);
        }

        [TestMethod]
        public void FindMatchTest_Type_TheSameSell()
        {
            var date = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Sell);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsFalse(isMatched);
        }

        [TestMethod]
        public void FindMatchTest_Date_NotMatchInSeconds()
        {
            DateTime date1 = new DateTime(2023, 5, 3, 11, 6, 0);
            DateTime date2 = new DateTime(2023, 5, 3, 11, 6, 1);


            if(DateTime.Compare(date1, date2) == 0)
            {
                Debug.WriteLine("Dates are equal");
            }
            else
            {
                Debug.WriteLine("Dates are not equal");
            }

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date1, Side.Sell);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date2, Side.Buy);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsFalse(isMatched);
        }

        [TestMethod]
        public void FindMatchTest_Date_NotMatchInSecondsAnotherOrder()
        {
            var date1 = DateTime.UtcNow;
            var date2 = DateTime.UtcNow;

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date2, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date1, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsFalse(isMatched);
        }

        /* FindMatchTest_MoreThen2IdendicalRecords 
        /  Requirements:This service finds matches between 2 records – each record can be matched to only one other record.
        */
        [TestMethod]
        public void FindMatchTest_MoreThen2IdendicalRecordsBuy()
        {
            var date = DateTime.UtcNow;

            var record = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Buy);

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsFalse(isMatched);
        }

        /* FindMatchTest_MoreThen2IdendicalRecords 
        /  Requirements:This service finds matches between 2 records – each record can be matched to only one other record.
        */
        [TestMethod]
        public void FindMatchTest_MoreThen2IdendicalRecordsSell()
        {
            var date = DateTime.UtcNow;

            _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Sell);
            _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Sell);
            _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Sell);
            _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Sell);
            _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Sell);

            var record1 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Buy);
            var record2 = _matchingApi.CreateRecord(Symbol.GOOG, 146, 10, date, Side.Sell);

            var isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            Assert.IsFalse(isMatched);
        }

   
    }
}