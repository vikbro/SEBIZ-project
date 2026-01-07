using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Domain.Contracts.MogoDbProductAPI.Domain.Contracts;
using SEBIZ.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SEBIZ.Service
{
    public class GameService : IGameService
    {
        private readonly IMongoCollection<Game> _gamesCollection;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GameService(IOptions<MongoDBSettings> mongoDBSettings, IWebHostEnvironment webHostEnvironment)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _gamesCollection = mongoDatabase.GetCollection<Game>(mongoDBSettings.Value.CollectionName);
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<GameDto> CreateGameAsync(CreateGameDto dto)
        {
            string imageUrl = await SaveImage(dto.ImageFile);

            var game = new Game
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Genre = dto.Genre,
                Developer = dto.Developer,
                ReleaseDate = dto.ReleaseDate,
                Tags = dto.Tags,
                ImageUrl = imageUrl
            };

            await _gamesCollection.InsertOneAsync(game);
            return new GameDto(game.Id, game.Name, game.Description, game.Price, game.Genre, game.Developer, game.ReleaseDate, game.Tags, game.ImageUrl);
        }

        public async Task DeleteGameAsync(string id)
        {
            var game = await _gamesCollection.Find(g => g.Id == id).FirstOrDefaultAsync();
            if (game == null)
            {
                throw new MongoException($"Game with id {id} not found");
            }

            if (!string.IsNullOrEmpty(game.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Images", "Games", Path.GetFileName(game.ImageUrl));
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
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
            return games.Select(g => new GameDto(g.Id, g.Name, g.Description, g.Price, g.Genre, g.Developer, g.ReleaseDate, g.Tags, g.ImageUrl));
        }

        public async Task<GameDto> GetGameByIdAsync(string id)
        {
            var game = await _gamesCollection.Find(g => g.Id == id).FirstOrDefaultAsync();
            if (game == null)
            {
                throw new MongoException($"Game with id {id} not found");
            }
            return new GameDto(game.Id, game.Name, game.Description, game.Price, game.Genre, game.Developer, game.ReleaseDate, game.Tags, game.ImageUrl);
        }

        public async Task<GameDto> UpdateGameAsync(string id, UpdateGameDto dto)
        {
            var game = await _gamesCollection.Find(g => g.Id == id).FirstOrDefaultAsync();
            if (game == null)
            {
                throw new MongoException($"Game with id {id} not found");
            }

            if (dto.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(game.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Images", "Games", Path.GetFileName(game.ImageUrl));
                    if (File.Exists(oldImagePath))
                    {
                        File.Delete(oldImagePath);
                    }
                }
                game.ImageUrl = await SaveImage(dto.ImageFile);
            }

            game.Name = dto.Name;
            game.Description = dto.Description;
            game.Price = dto.Price;
            game.Genre = dto.Genre;
            game.Developer = dto.Developer;
            game.ReleaseDate = dto.ReleaseDate;
            game.Tags = dto.Tags;

            await _gamesCollection.ReplaceOneAsync(g => g.Id == id, game);
            return new GameDto(game.Id, game.Name, game.Description, game.Price, game.Genre, game.Developer, game.ReleaseDate, game.Tags, game.ImageUrl);
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return null;
            }

            if (imageFile.Length > 5 * 1024 * 1024)
            {
                throw new Exception("File size should not exceed 5MB");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception("Only .jpg, .jpeg, and .png formats are allowed.");
            }

            var contentPath = _webHostEnvironment.ContentRootPath;
            var path = Path.Combine(contentPath, "Images", "Games");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(path, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/Images/Games/{fileName}";
        }
    }
}
