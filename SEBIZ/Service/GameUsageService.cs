using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Domain.Models;
using System;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public class GameUsageService : IGameUsageService
    {
        private readonly IMongoCollection<GameUsage> _gameUsageCollection;

        public GameUsageService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _gameUsageCollection = mongoDatabase.GetCollection<GameUsage>(mongoDBSettings.Value.GameUsageCollectionName);
        }

        public async Task UpdatePlayTimeAsync(string userId, string gameId, int secondsPlayed)
        {
            var filter = Builders<GameUsage>.Filter.Where(gu => gu.UserId == userId && gu.GameId == gameId);
            var existingUsage = await _gameUsageCollection.Find(filter).FirstOrDefaultAsync();

            if (existingUsage != null)
            {
                existingUsage.PlayTimeSeconds += secondsPlayed;
                existingUsage.LastPlayed = DateTime.UtcNow;
                await _gameUsageCollection.ReplaceOneAsync(filter, existingUsage);
            }
            else
            {
                var newUsage = new GameUsage
                {
                    UserId = userId,
                    GameId = gameId,
                    PlayTimeSeconds = secondsPlayed,
                    LastPlayed = DateTime.UtcNow
                };
                await _gameUsageCollection.InsertOneAsync(newUsage);
            }
        }

        public async Task<IEnumerable<GameUsage>> GetGameUsagesByUserId(string userId)
        {
            var filter = Builders<GameUsage>.Filter.Eq(gu => gu.UserId, userId);
            return await _gameUsageCollection.Find(filter).ToListAsync();
        }
    }
}
