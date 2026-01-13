using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Domain.Models;

namespace SEBIZ.Data
{
    public class AppDbContext
    {
        public IMongoCollection<Game> GameCollection { get; }
        public IMongoCollection<GameUsage> GameUsages { get; }
        public IMongoCollection<User> Users { get; }

        public AppDbContext(IOptions<MongoDBSettings> mongoDbSettings)
        {
            MongoClient mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionURI);
            IMongoDatabase database = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);

            GameCollection = database.GetCollection<Game>(mongoDbSettings.Value.GamesCollectionName);
            GameUsages = database.GetCollection<GameUsage>(mongoDbSettings.Value.GameUsageCollectionName);
            Users = database.GetCollection<User>(mongoDbSettings.Value.UsersCollectionName);
        }
    }
}
