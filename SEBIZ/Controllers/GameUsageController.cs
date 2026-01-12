using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Service;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SEBIZ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameUsageController : ControllerBase
    {
        private readonly IGameUsageService _gameUsageService;
        private readonly IGameService _gameService;
        private readonly ILogger<GameUsageController> _logger;

        public GameUsageController(IGameUsageService gameUsageService, IGameService gameService, ILogger<GameUsageController> logger)
        {
            _gameUsageService = gameUsageService;
            _gameService = gameService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("WithGameDetails/me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<GameUsageWithTitleDto>>> GetGameUsagesWithGameDetails()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var gameUsages = await _gameUsageService.GetGameUsagesByUserId(userId);
                var gameIds = gameUsages.Select(gu => gu.GameId).Distinct();
                var games = await _gameService.GetGamesByIdsAsync(gameIds);

                var result = gameUsages.Select(gu => new GameUsageWithTitleDto
                {
                    GameId = gu.GameId,
                    PlayTimeMinutes = (long)Math.Round(gu.PlayTimeSeconds / 60.0),
                    LastPlayed = gu.LastPlayed,
                    GameTitle = games.FirstOrDefault(g => g.Id == gu.GameId)?.Name ?? "Unknown"
                });

                return Ok(result);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error retrieving game usages for user {UserId}", userId);
                return BadRequest(ex.Message);
            }
        }
    }

    public class GameUsageWithTitleDto
    {
        public string GameId { get; set; }
        public long PlayTimeMinutes { get; set; }
        public System.DateTime LastPlayed { get; set; }
        public string GameTitle { get; set; }
    }
}
