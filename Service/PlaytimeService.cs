using MongoDB.Driver;
using SEBIZ.Data;
using SEBIZ.Domain.Models;

namespace SEBIZ.Service
{
    public class PlaytimeService : IPlaytimeService
    {
        private readonly IMongoCollection<PlaytimeRecord> _playtimeRecords;

        public PlaytimeService(AppDbContext dbContext)
        {
            _playtimeRecords = dbContext.PlaytimeRecords;
        }

        public async Task StartPlaytimeAsync(string userId, string gameId)
        {
            var record = new PlaytimeRecord
            {
                UserId = userId,
                GameId = gameId,
                StartTime = DateTime.UtcNow
            };
            await _playtimeRecords.InsertOneAsync(record);
        }

        public async Task StopPlaytimeAsync(string userId, string gameId)
        {
            var filter = Builders<PlaytimeRecord>.Filter.Eq(r => r.UserId, userId) &
                         Builders<PlaytimeRecord>.Filter.Eq(r => r.GameId, gameId) &
                         Builders<PlaytimeRecord>.Filter.Eq(r => r.EndTime, null);
            var update = Builders<PlaytimeRecord>.Update.Set(r => r.EndTime, DateTime.UtcNow);

            await _playtimeRecords.UpdateOneAsync(filter, update);
        }
    }
}
