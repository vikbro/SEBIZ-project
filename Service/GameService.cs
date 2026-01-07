using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Domain.Contracts.MogoDbProductAPI.Domain.Contracts;
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
            _gamesCollection = mongoDatabase.GetCollection<Game>(mongoDBSettings.Value.CollectionName);
        }

        public async Task<GameDto> CreateGameAsync(CreateGameDto dto)
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
                ImagePath = string.IsNullOrEmpty(dto.ImagePath) ? "Images/placeholder.png" : dto.ImagePath
            };

            await _gamesCollection.InsertOneAsync(game);
            return new GameDto(game.Id, game.Name, game.Description, game.Price, game.Genre, game.Developer, game.ReleaseDate, game.Tags, game.ImagePath);
        }

        public async Task DeleteGameAsync(string id)
        {
            var game = await _gamesCollection.FindOneAndDeleteAsync(g => g.Id == id);
            if (game == null)
            {
                throw new MongoException($"Game with id {id} not found");
            }

            if (!string.IsNullOrEmpty(game.ImagePath) && game.ImagePath != "Images/placeholder.png")
            {
                if (System.IO.File.Exists(game.ImagePath))
                {
                    System.IO.File.Delete(game.ImagePath);
                }
            }
        }

        public async Task<IEnumerable<GameDto>> GetAllGamesAsync()
        {
            var games = await _gamesCollection.Find(_ => true).ToListAsync();
            return games.Select(g => new GameDto(g.Id, g.Name, g.Description, g.Price, g.Genre, g.Developer, g.ReleaseDate, g.Tags, g.ImagePath));
        }

        public async Task<GameDto> GetGameByIdAsync(string id)
        {
            var game = await _gamesCollection.Find(g => g.Id == id).FirstOrDefaultAsync();
            if (game == null)
            {
                throw new MongoException($"Game with id {id} not found");
            }
            return new GameDto(game.Id, game.Name, game.Description, game.Price, game.Genre, game.Developer, game.ReleaseDate, game.Tags, game.ImagePath);
        }

        public async Task<GameDto> UpdateGameAsync(string id, UpdateGameDto dto)
        {
            var updatedGame = new Game
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Genre = dto.Genre,
                Developer = dto.Developer,
                ReleaseDate = dto.ReleaseDate,
                Tags = dto.Tags,
                ImagePath = string.IsNullOrEmpty(dto.ImagePath) ? "Images/placeholder.png" : dto.ImagePath
            };

            var oldGame = await _gamesCollection.FindOneAndReplaceAsync(
                Builders<Game>.Filter.Eq(g => g.Id, id),
                updatedGame,
                new FindOneAndReplaceOptions<Game> { ReturnDocument = ReturnDocument.Before }
            );

            if (oldGame == null)
            {
                throw new MongoException($"Game with id {id} not found");
            }

            // Delete old image if it's different from the new one and not the placeholder
            if (oldGame.ImagePath != updatedGame.ImagePath && oldGame.ImagePath != "Images/placeholder.png")
            {
                if (System.IO.File.Exists(oldGame.ImagePath))
                {
                    System.IO.File.Delete(oldGame.ImagePath);
                }
            }

            return new GameDto(updatedGame.Id, updatedGame.Name, updatedGame.Description, updatedGame.Price, updatedGame.Genre, updatedGame.Developer, updatedGame.ReleaseDate, updatedGame.Tags, updatedGame.ImagePath);
        }
    }
}
