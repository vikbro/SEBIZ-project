using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Domain.Models;

namespace SEBIZ.Data
{
    public class AppDbContext
    {
        public IMongoCollection<Game> GameCollection { get; }

        public AppDbContext(IOptions<MongoDBSettings> mongoDbSettings)
        {
            //This gives us connecton to MongoDB server
            MongoClient mongoClient = new MongoClient(
                mongoDbSettings.Value.ConnectionURI);

            //Conntecting to specific database
            IMongoDatabase database = mongoClient.GetDatabase(
                mongoDbSettings.Value.DatabaseName);

            //Connecting to specific collection
            GameCollection = database.GetCollection<Game>(
                mongoDbSettings.Value.CollectionName);

            var mongoDatabase = mongoClient.GetDatabase(
                mongoDbSettings.Value.DatabaseName);

            GameCollection = mongoDatabase.GetCollection<Game>(
                mongoDbSettings.Value.CollectionName);
        }
    }
}
