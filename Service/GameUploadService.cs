using Microsoft.AspNetCore.Http;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using SEBIZ.Data;
using MongoDB.Driver;
using SEBIZ.Domain.Models;
using Microsoft.Extensions.Options;
using SEBIZ.Domain.Contracts;
using Microsoft.AspNetCore.Hosting;

namespace SEBIZ.Service
{
    public class GameUploadService : IGameUploadService
    {
        private readonly IMongoCollection<Game> _games;
        private readonly MongoDBSettings _mongoDbSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GameUploadService(AppDbContext context, IOptions<MongoDBSettings> mongoDbSettings, IWebHostEnvironment webHostEnvironment)
        {
            _games = context.GameCollection;
            _mongoDbSettings = mongoDbSettings.Value;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> UploadGame(string gameId, IFormFile file)
        {
            // Sanitize gameId to prevent directory traversal
            var sanitizedGameId = Path.GetFileName(gameId);
            var webRootPath = Path.Combine(_webHostEnvironment.WebRootPath, _mongoDbSettings.GameFilePath);
            if (!Directory.Exists(webRootPath))
            {
                Directory.CreateDirectory(webRootPath);
            }

            var gameFolderPath = Path.Combine(webRootPath, sanitizedGameId);
            if (Directory.Exists(gameFolderPath))
            {
                Directory.Delete(gameFolderPath, true);
            }
            Directory.CreateDirectory(gameFolderPath);

            var filePath = Path.Combine(gameFolderPath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            ZipFile.ExtractToDirectory(filePath, gameFolderPath);

            File.Delete(filePath);

            var gameUrl = $"/games/{gameId}/index.html";

            var filter = Builders<Game>.Filter.Eq(g => g.Id, gameId);
            var update = Builders<Game>.Update.Set(g => g.GameUrl, gameUrl);
            await _games.UpdateOneAsync(filter, update);

            return gameUrl;
        }
    }
}
