using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace AF.AutomationTest.MatchingEngine.Tests
{     public class MongoDBService
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public MongoDBService(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<BsonDocument>(collectionName);
        }

        public async Task<List<BsonDocument>> GetJsonObjectsAsync()
        {
            var filter = Builders<BsonDocument>.Filter.Empty;
            var documents = await _collection.Find(filter).ToListAsync();
            return documents;
        }
    }
}