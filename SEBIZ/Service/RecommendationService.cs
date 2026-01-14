using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Game> _gamesCollection;

        public RecommendationService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _usersCollection = mongoDatabase.GetCollection<User>(mongoDBSettings.Value.UsersCollectionName);
            _gamesCollection = mongoDatabase.GetCollection<Game>(mongoDBSettings.Value.GamesCollectionName);
        }

        public async Task<IEnumerable<GameDto>> GetRecommendationsAsync(string userId)
        {
            var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null || user.OwnedGamesIds == null || !user.OwnedGamesIds.Any())
            {
                return Enumerable.Empty<GameDto>();
            }

            var ownedGames = await _gamesCollection.Find(g => user.OwnedGamesIds.Contains(g.Id)).ToListAsync();
            if (!ownedGames.Any())
            {
                return Enumerable.Empty<GameDto>();
            }

            var favoriteGenres = ownedGames
                .Where(g => g.Genre != null)
                .GroupBy(g => g.Genre)
                .OrderByDescending(grp => grp.Count())
                .Select(grp => grp.Key)
                .Take(3) // Take top 3 genres
                .ToList();

            if (!favoriteGenres.Any())
            {
                return Enumerable.Empty<GameDto>();
            }

            var recommendedGames = await _gamesCollection.Find(
                g => favoriteGenres.Contains(g.Genre) && !user.OwnedGamesIds.Contains(g.Id)
            ).Limit(10).ToListAsync();

            return recommendedGames.Select(g => new GameDto(g.Id, g.Name, g.Description, g.Price, g.Genre, g.Developer, g.ReleaseDate, g.Tags, g.ImagePath, g.CreatedById, g.FileName, g.GameFilePath, g.GameFileName));
        }
    }
}
