using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AF.AutomationTest.MatchingEngine.Tests
{
    [TestClass]
    public class MongoDBTests
    {
        private MongoDBService _mongoDBService;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            // Initialize MongoDBService with your connection details
            _mongoDBService = new MongoDBService("your_connection_string", "your_database_name", "your_collection_name");
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
