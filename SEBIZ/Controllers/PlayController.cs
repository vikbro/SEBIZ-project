using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEBIZ.Domain.Contracts;
using SEBIZ.Service;
using System.IO;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SEBIZ.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class PlayController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, object> _gameLocks = new ConcurrentDictionary<string, object>();
        private readonly IGameService _gameService;
        private readonly IGameUsageService _gameUsageService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<PlayController> _logger;

        public PlayController(IGameService gameService, IGameUsageService gameUsageService, IUserService userService, IWebHostEnvironment hostingEnvironment, ILogger<PlayController> logger)
        {
            _gameService = gameService;
            _gameUsageService = gameUsageService;
            _userService = userService;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        [HttpGet("{gameId}")]
        public IActionResult Play(string gameId)
        {
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "views", "play.html");
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }
            return PhysicalFile(filePath, "text/html");
        }

        [HttpGet("Content/{gameId}/{*filePath}")]
        public async Task<IActionResult> GameContent(string gameId, string filePath)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null || !user.OwnedGamesIds.Contains(gameId))
            {
                return Forbid();
            }

            var game = await _gameService.GetGameByIdAsync(gameId);
            if (game == null)
            {
                return NotFound();
            }

            var webRootPath = _hostingEnvironment.WebRootPath;
            var gameZipPath = Path.Combine(webRootPath, "games", game.FileName);

            if (string.IsNullOrEmpty(game.FileName) || !System.IO.File.Exists(gameZipPath))
            {
                _logger.LogError("Game zip file not found for game: {GameTitle}", game.Name);
                return NotFound("Game file not found.");
            }

            var tempExtractPath = Path.Combine(Path.GetTempPath(), "SEBIZGames", gameId);
            var gameLock = _gameLocks.GetOrAdd(gameId, new object());

            lock (gameLock)
            {
                if (!Directory.Exists(tempExtractPath))
                {
                    Directory.CreateDirectory(tempExtractPath);
                    ZipFile.ExtractToDirectory(gameZipPath, tempExtractPath);
                }
            }

            var requestedFilePath = string.IsNullOrEmpty(filePath)
                ? Path.Combine(tempExtractPath, "index.html")
                : Path.Combine(tempExtractPath, filePath);

            var fullPath = Path.GetFullPath(requestedFilePath);
            if (!fullPath.StartsWith(Path.GetFullPath(tempExtractPath)) || !System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var contentType = GetContentType(requestedFilePath);
            return PhysicalFile(requestedFilePath, contentType);
        }

        private static readonly Dictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            {".html", "text/html"},
            {".js", "application/javascript"},
            {".css", "text/css"},
            {".png", "image/png"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".gif", "image/gif"},
            {".wasm", "application/wasm"},
        };

        private static string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return _mimeTypeMappings.TryGetValue(ext, out var contentType) ? contentType : "application/octet-stream";
        }

        [HttpPost("playtime/update")]
        public async Task<IActionResult> UpdatePlaytime([FromBody] PlaytimeUpdateDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await _gameUsageService.UpdatePlayTimeAsync(userId, dto.GameId, dto.SecondsPlayed);
            return Ok();
        }
    }

    public class PlaytimeUpdateDto
    {
        public string GameId { get; set; }
        public int SecondsPlayed { get; set; }
    }
}
