using MongoDB.Driver;
using System.Threading.Tasks;
using SEBIZ.Data;
using SEBIZ.Domain.Models;

namespace SEBIZ.Service
{
    public class GameUsageService : IGameUsageService
    {
        private readonly IMongoCollection<GameUsage> _gameUsages;

        public GameUsageService(AppDbContext context)
        {
            _gameUsages = context.GameUsages;
        }

        public async Task UpdatePlayTime(GameUsage gameUsage)
        {
            var filter = Builders<GameUsage>.Filter.And(
                Builders<GameUsage>.Filter.Eq(u => u.UserId, gameUsage.UserId),
                Builders<GameUsage>.Filter.Eq(u => u.GameId, gameUsage.GameId)
            );

            var existingUsage = await _gameUsages.Find(filter).FirstOrDefaultAsync();

            if (existingUsage != null)
            {
                var update = Builders<GameUsage>.Update.Inc(u => u.MinutesPlayed, gameUsage.MinutesPlayed);
                await _gameUsages.UpdateOneAsync(filter, update);
            }
            else
            {
                await _gameUsages.InsertOneAsync(gameUsage);
            }
        }
    }
}
