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
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public GameService(IOptions<MongoDBSettings> mongoDBSettings, HttpClient httpClient, IConfiguration configuration)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _gamesCollection = mongoDatabase.GetCollection<Game>(mongoDBSettings.Value.GamesCollectionName);
            _httpClient = httpClient;
            _baseUrl = configuration["BaseUrl"] ?? "http://localhost:5000";
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
                CreatedById = userId,
                ImagePath = dto.ImagePath,
                FileName = dto.FileName,
                GameFilePath = dto.GameFilePath,
                GameFileName = dto.GameFileName
            };

            await _gamesCollection.InsertOneAsync(game);
            return new GameDto(game.Id, game.Name, game.Description, game.Price, game.Genre, game.Developer, game.ReleaseDate, game.Tags, game.ImagePath, game.CreatedById, game.FileName, game.GameFilePath, game.GameFileName);
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

            // Delete game file if exists
            if (!string.IsNullOrEmpty(game.GameFileName))
            {
                try
                {
                    var deleteUrl = $"{_baseUrl}/api/GameFile/delete?fileName={game.GameFileName}";
                    await _httpClient.DeleteAsync(deleteUrl);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting game file: {ex.Message}");
                }
            }

            // Delete image file if exists
            if (!string.IsNullOrEmpty(game.FileName))
            {
                try
                {
                    var deleteUrl = $"{_baseUrl}/api/GameFile/image-delete?fileName={game.FileName}";
                    await _httpClient.DeleteAsync(deleteUrl);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting image file: {ex.Message}");
                }
            }
        }

        public async Task<IEnumerable<GameDto>> GetAllGamesAsync()
        {
            var games = await _gamesCollection.Find(_ => true).ToListAsync();
            return games.Select(g => new GameDto(g.Id, g.Name, g.Description, g.Price, g.Genre, g.Developer, g.ReleaseDate, g.Tags, g.ImagePath, g.CreatedById, g.FileName, g.GameFilePath, g.GameFileName));
        }

        public async Task<GameDto> GetGameByIdAsync(string id)
        {
            var game = await _gamesCollection.Find(g => g.Id == id).FirstOrDefaultAsync();
            if (game == null)
            {
                throw new MongoException($"Game with id {id} not found");
            }
            return new GameDto(game.Id, game.Name, game.Description, game.Price, game.Genre, game.Developer, game.ReleaseDate, game.Tags, game.ImagePath, game.CreatedById, game.FileName, game.GameFilePath, game.GameFileName);
        }

        public async Task<IEnumerable<GameDto>> GetGamesByIdsAsync(IEnumerable<string> ids)
        {
            var filter = Builders<Game>.Filter.In(g => g.Id, ids);
            var games = await _gamesCollection.Find(filter).ToListAsync();
            return games.Select(g => new GameDto(g.Id, g.Name, g.Description, g.Price, g.Genre, g.Developer, g.ReleaseDate, g.Tags, g.ImagePath, g.CreatedById, g.FileName, g.GameFilePath, g.GameFileName));
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

            // If new game file is provided, delete old one
            if (!string.IsNullOrEmpty(dto.GameFileName) && dto.GameFileName != game.GameFileName && !string.IsNullOrEmpty(game.GameFileName))
            {
                try
                {
                    var deleteUrl = $"{_baseUrl}/api/GameFile/delete?fileName={game.GameFileName}";
                    await _httpClient.DeleteAsync(deleteUrl);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting old game file: {ex.Message}");
                }
            }

            // If new image file is provided, delete old one
            if (!string.IsNullOrEmpty(dto.FileName) && dto.FileName != game.FileName && !string.IsNullOrEmpty(game.FileName))
            {
                try
                {
                    var deleteUrl = $"{_baseUrl}/api/GameFile/image-delete?fileName={game.FileName}";
                    await _httpClient.DeleteAsync(deleteUrl);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting old image file: {ex.Message}");
                }
            }

            game.Name = dto.Name;
            game.Description = dto.Description;
            game.Price = dto.Price;
            game.Genre = dto.Genre;
            game.Developer = dto.Developer;
            game.ReleaseDate = dto.ReleaseDate;
            game.Tags = dto.Tags;
            game.ImagePath = dto.ImagePath;
            game.FileName = dto.FileName;
            game.GameFilePath = dto.GameFilePath;
            game.GameFileName = dto.GameFileName;

            await _gamesCollection.ReplaceOneAsync(g => g.Id == id, game);
            return new GameDto(game.Id, game.Name, game.Description, game.Price, game.Genre, game.Developer, game.ReleaseDate, game.Tags, game.ImagePath, game.CreatedById, game.FileName, game.GameFilePath, game.GameFileName);
        }

        public async Task DeleteGameAsAdminAsync(string id)
        {
            var game = await _gamesCollection.Find(g => g.Id == id).FirstOrDefaultAsync();
            if (game == null)
            {
                throw new MongoException($"Game with id {id} not found");
            }

            var result = await _gamesCollection.DeleteOneAsync(g => g.Id == id);
            if (result.DeletedCount == 0)
            {
                throw new MongoException($"Game with id {id} not found");
            }

            // Delete game file if exists
            if (!string.IsNullOrEmpty(game.GameFileName))
            {
                try
                {
                    var deleteUrl = $"{_baseUrl}/api/GameFile/delete?fileName={game.GameFileName}";
                    await _httpClient.DeleteAsync(deleteUrl);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting game file: {ex.Message}");
                }
            }

            // Delete image file if exists
            if (!string.IsNullOrEmpty(game.FileName))
            {
                try
                {
                    var deleteUrl = $"{_baseUrl}/api/GameFile/image-delete?fileName={game.FileName}";
                    await _httpClient.DeleteAsync(deleteUrl);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting image file: {ex.Message}");
                }
            }
        }

        public async Task<GameDto> UpdateGameAsAdminAsync(string id, UpdateGameDto dto)
        {
            var game = await _gamesCollection.Find(g => g.Id == id).FirstOrDefaultAsync();
            if (game == null)
            {
                throw new MongoException($"Game with id {id} not found");
            }

            // If new game file is provided, delete old one
            if (!string.IsNullOrEmpty(dto.GameFileName) && dto.GameFileName != game.GameFileName && !string.IsNullOrEmpty(game.GameFileName))
            {
                try
                {
                    var deleteUrl = $"{_baseUrl}/api/GameFile/delete?fileName={game.GameFileName}";
                    await _httpClient.DeleteAsync(deleteUrl);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting old game file: {ex.Message}");
                }
            }

            // If new image file is provided, delete old one
            if (!string.IsNullOrEmpty(dto.FileName) && dto.FileName != game.FileName && !string.IsNullOrEmpty(game.FileName))
            {
                try
                {
                    var deleteUrl = $"{_baseUrl}/api/GameFile/image-delete?fileName={game.FileName}";
                    await _httpClient.DeleteAsync(deleteUrl);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting old image file: {ex.Message}");
                }
            }

            game.Name = dto.Name;
            game.Description = dto.Description;
            game.Price = dto.Price;
            game.Genre = dto.Genre;
            game.Developer = dto.Developer;
            game.ReleaseDate = dto.ReleaseDate;
            game.Tags = dto.Tags;
            game.ImagePath = dto.ImagePath;
            game.FileName = dto.FileName;
            game.GameFilePath = dto.GameFilePath;
            game.GameFileName = dto.GameFileName;

            await _gamesCollection.ReplaceOneAsync(g => g.Id == id, game);
            return new GameDto(game.Id, game.Name, game.Description, game.Price, game.Genre, game.Developer, game.ReleaseDate, game.Tags, game.ImagePath, game.CreatedById, game.FileName, game.GameFilePath, game.GameFileName);
        }
    }
}
