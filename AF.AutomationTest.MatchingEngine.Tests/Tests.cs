using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Newtonsoft.Json;
using SharpCompress.Common;

namespace AF.AutomationTest.MatchingEngine.Tests
{
    public class TradeRecord
    {
        public Id _id { get; set; }
        public List<Record> Records { get; set; } = new List<Record>();
        public string compareRecords { get; set; }
        public bool expectedMatch { get; set; }
        public string DisplayName { get; set; }
    }

    public class Id
    {
        public string _id { get; set; }
    }
    
[TestClass]
    public class Tests
    {
        private static MatchingApi _matchingApi;
        private MongoDBService _mongoDBService;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            _matchingApi = new MatchingApi();
        }
        
        [TestInitialize]
        public void TestInitialize()
        {
            _matchingApi.ClearData();

            // Initialize MongoDBService with your connection details
            _mongoDBService = new MongoDBService(
                "mongodb+srv://anatolyg:p6wQun9b5IfXn6c2@cluster0-anatolyg.7v9it.mongodb.net/Cluster0-anatolyg?retryWrites=true&w=majority",
                "aft_qa_automation",
                "trades_to_Verify");


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
        [DynamicData(nameof(GetTestCasesFromJsonAny3), DynamicDataSourceType.Method)]
        public void FindMatchTestFromJsonRecordsAny(string compareRecords, bool expectedMatch, string displayName,
            (Symbol symbol, int quantity, double price, DateTime date, Side side)[] records)
        {
            Debug.WriteLine("FindMatchTestFromJsonRecordsAny started");
            var createdRecords = records.Select(r => CreateRecord(r.symbol, r.quantity, r.price, r.date, r.side)).ToList();

            // Parse the compareRecords string to get the indices
            var indices = compareRecords.Split(',').Select(int.Parse).ToArray();
            if (indices.Length != 2)
            {
                throw new ArgumentException("compareRecords must specify exactly two indices.");
            }

            // Get the records to compare
            var record1 = createdRecords[indices[0] - 1]; // Convert to zero-based index
            var record2 = createdRecords[indices[1] - 1]; // Convert to zero-based index

            // Check if the two specified records match
            bool isMatched = _matchingApi.CheckIfRecordsMatched(record1, record2);

            // Assert if the result matches the expected outcome
            Assert.AreEqual(expectedMatch, isMatched, displayName);
        }

        public static IEnumerable<object[]> GetTestCasesFromJsonAny3()
        {
            Debug.WriteLine("GetTestCasesFromJsonAny started");

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            Debug.WriteLine($"Base path: {basePath}");

            var filePath = Path.Combine(basePath, "..", "..", "..", "TestData", "aft_qa_automation.trades_to_Verify.json");
            Debug.WriteLine($"File path: {filePath}");

            var jsonContent = File.ReadAllText(filePath);
            Debug.WriteLine("JSON content read");

            var testCases = new List<object[]>();

            try
            {
                var records = JsonConvert.DeserializeObject<List<TradeRecord>>(jsonContent);
                Debug.WriteLine("JSON deserialized");

                if (records == null || !records.Any())
                {
                    Debug.WriteLine("No records found in JSON file.");
                }
                else
                {
                    foreach (var record in records)
                    {
                        var recordArray = record.Records.Select(
                            r =>((Symbol)Enum.Parse(typeof(Symbol), r.Symbol.ToString()), r.Quantity, r.Price, r.SettlementDate, (Side)Enum.Parse(typeof(Side), r.Side.ToString()))
                        ).ToArray();

                        var testCase = new object[]
                        {
                            record.compareRecords,
                            record.expectedMatch,
                            record.DisplayName,
                            recordArray  // Pass the entire array as a single object
                         };

                        testCases.Add(testCase);
                        Debug.WriteLine($"Added test case: {record.DisplayName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception during JSON deserialization: {ex.Message}");
            }

            Debug.WriteLine("GetTestCasesFromJsonAny completed");
            return testCases;
        }

        [DataTestMethod]
        [DynamicData(nameof(GetTestCases), DynamicDataSourceType.Method)]
        public void FindMatchTestFromFile(bool expectedMatch, string displayName, params (string symbol, int quantity, double price, string dateString, Side side)[] records)
        {
            var createdRecords = records.Select(r => CreateRecord(r.symbol, r.quantity, r.price, DateTime.Parse(r.dateString), r.side)).ToList();

            // Check if records match
            bool isMatched = false;
            for (int i = 0; i < createdRecords.Count; i++)
            {
                for (int j = i + 1; j < createdRecords.Count; j++)
                {
                    if (_matchingApi.CheckIfRecordsMatched(createdRecords[i], createdRecords[j]))
                    {
                        isMatched = true;
                        break;
                    }
                }
                if (isMatched) break;
            }

            // Assert if the result matches the expected outcome
            Assert.AreEqual(expectedMatch, isMatched, displayName);
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

        private Record CreateRecord(Symbol symbol, int quantity, double price, DateTime date, Side side)
        {
            return _matchingApi.CreateRecord(symbol, quantity, price, date, side);
        }

        // Helper method for creating records
        private Record CreateRecord(string symbol, int quantity, double price, DateTime date, Side side)
        {
            return _matchingApi.CreateRecord((Symbol)Enum.Parse(typeof(Symbol), symbol), quantity, price, date, side);
        }
        public static IEnumerable<object[]> GetTestCases()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(basePath,"..", "..", "..", "TestData", "testdataMy.csv");
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