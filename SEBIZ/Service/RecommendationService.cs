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

            // Split comma-separated genres and flatten them
            var genreCount = new Dictionary<string, int>();
            foreach (var game in ownedGames)
            {
                if (!string.IsNullOrEmpty(game.Genre))
                {
                    var genres = game.Genre.Split(',').Select(g => g.Trim()).Where(g => !string.IsNullOrEmpty(g));
                    foreach (var genre in genres)
                    {
                        if (genreCount.ContainsKey(genre))
                            genreCount[genre]++;
                        else
                            genreCount[genre] = 1;
                    }
                }
            }

            if (!genreCount.Any())
            {
                return Enumerable.Empty<GameDto>();
            }

            var favoriteGenres = genreCount
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .Take(3) // Take top 3 genres
                .ToList();

            // Get all games not owned by user
            var allGames = await _gamesCollection.Find(g => !user.OwnedGamesIds.Contains(g.Id)).ToListAsync();

            // Filter games that contain at least one of the favorite genres
            var recommendedGames = allGames
                .Where(g => !string.IsNullOrEmpty(g.Genre) && 
                    g.Genre.Split(',')
                        .Select(genre => genre.Trim())
                        .Any(genre => favoriteGenres.Contains(genre)))
                .Take(3)
                .ToList();

            return recommendedGames.Select(g => new GameDto(g.Id, g.Name, g.Description, g.Price, g.Genre, g.Developer, g.ReleaseDate, g.Tags, g.ImagePath, g.CreatedById, g.FileName, g.GameFilePath, g.GameFileName));
        }
    }
}
