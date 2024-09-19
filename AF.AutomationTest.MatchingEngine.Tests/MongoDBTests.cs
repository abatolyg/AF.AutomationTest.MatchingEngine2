using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AF.AutomationTest.MatchingEngine.Tests
{
    [TestClass]
    public class MongoDBTests
    {
        private static MongoDBService _mongoDBService;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            // Initialize MongoDBService with your connection details
            _mongoDBService = new MongoDBService(
                "mongodb+srv://anatolyg:p6wQun9b5IfXn6c2@cluster0-anatolyg.7v9it.mongodb.net/Cluster0-anatolyg?retryWrites=true&w=majority",
                "aft_qa_automation",
                "trades_to_Verify");
        }

        [TestMethod]
        public async Task TestReadJsonObjectsFromMongoDB()
        {
            List<BsonDocument> jsonObjects = await _mongoDBService.GetJsonObjectsAsync();

            // Assert that the list is not empty
            Assert.IsTrue(jsonObjects.Count > 0);

            // Optionally, you can perform more specific assertions based on your requirements
        }
    }
}
