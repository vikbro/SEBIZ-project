using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public class GameService : IGameService
    {
        private readonly IMongoCollection<Game> _gamesCollection;

        public GameService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _gamesCollection = mongoDatabase.GetCollection<Game>(mongoDBSettings.Value.GamesCollectionName);
        }

        public async Task<GameDto> CreateGameAsync(CreateGameDto dto, string userId)
        {
            var game = new Game
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Genre = dto.Genre,
                Developer = dto.Developer,
                ReleaseDate = dto.ReleaseDate,
                Tags = dto.Tags,
                CreatedById = userId
            };

            await _gamesCollection.InsertOneAsync(game);
            return new GameDto(game.Id, game.Name, game.Description, game.Price, game.Genre, game.Developer, game.ReleaseDate, game.Tags);
        }

        public async Task DeleteGameAsync(string id, string userId)
        {
            var game = await _gamesCollection.Find(g => g.Id == id).FirstOrDefaultAsync();
            if (game == null)
            {
                throw new MongoException($"Game with id {id} not found");
            }

            if (game.CreatedById != userId)
            {
                throw new System.UnauthorizedAccessException("User is not authorized to delete this game");
            }

            var result = await _gamesCollection.DeleteOneAsync(g => g.Id == id);
            if (result.DeletedCount == 0)
            {
                throw new MongoException($"Game with id {id} not found");
            }
        }

        public async Task<IEnumerable<GameDto>> GetAllGamesAsync()
        {
            var games = await _gamesCollection.Find(_ => true).ToListAsync();
            return games.Select(g => new GameDto(g.Id, g.Name, g.Description, g.Price, g.Genre, g.Developer, g.ReleaseDate, g.Tags));
        }

        public async Task<GameDto> GetGameByIdAsync(string id)
        {
            var game = await _gamesCollection.Find(g => g.Id == id).FirstOrDefaultAsync();
            if (game == null)
            {
                throw new MongoException($"Game with id {id} not found");
            }
            return new GameDto(game.Id, game.Name, game.Description, game.Price, game.Genre, game.Developer, game.ReleaseDate, game.Tags);
        }

        public async Task<GameDto> UpdateGameAsync(string id, UpdateGameDto dto, string userId)
        {
            var game = await _gamesCollection.Find(g => g.Id == id).FirstOrDefaultAsync();
            if (game == null)
            {
                throw new MongoException($"Game with id {id} not found");
            }

            if (game.CreatedById != userId)
            {
                throw new System.UnauthorizedAccessException("User is not authorized to update this game");
            }

            game.Name = dto.Name;
            game.Description = dto.Description;
            game.Price = dto.Price;
            game.Genre = dto.Genre;
            game.Developer = dto.Developer;
            game.ReleaseDate = dto.ReleaseDate;
            game.Tags = dto.Tags;

            await _gamesCollection.ReplaceOneAsync(g => g.Id == id, game);
            return new GameDto(game.Id, game.Name, game.Description, game.Price, game.Genre, game.Developer, game.ReleaseDate, game.Tags);
        }
    }
}
