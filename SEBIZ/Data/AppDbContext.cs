using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Domain.Models;

namespace SEBIZ.Data
{
    public class AppDbContext
    {
        public IMongoCollection<Product> ProductCollection { get; }

        public AppDbContext(IOptions<MongoDBSettings> mongoDbSettings)
        {
            //This gives us connecton to MongoDB server
            MongoClient mongoClient = new MongoClient(
                mongoDbSettings.Value.ConnectionURI);

            //Conntecting to specific database
            IMongoDatabase database = mongoClient.GetDatabase(
                mongoDbSettings.Value.DatabaseName);

            //Connecting to specific collection
            ProductCollection = database.GetCollection<Product>(
                mongoDbSettings.Value.CollectionName);

            var mongoDatabase = mongoClient.GetDatabase(
                mongoDbSettings.Value.DatabaseName);

            ProductCollection = mongoDatabase.GetCollection<Product>(
                mongoDbSettings.Value.CollectionName);
        }
    }
}
