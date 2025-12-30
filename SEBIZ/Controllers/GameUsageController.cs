using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SEBIZ.Service;
using System.Threading.Tasks;

namespace SEBIZ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameUsageController : ControllerBase
    {
        private readonly IGameUsageService _gameUsageService;
        private readonly ILogger<GameUsageController> _logger;

        public GameUsageController(IGameUsageService gameUsageService, ILogger<GameUsageController> logger)
        {
            _gameUsageService = gameUsageService;
            _logger = logger;
        }

        [HttpPost("update")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePlayTime([FromBody] UpdatePlayTimeDto dto)
        {
            try
            {
                await _gameUsageService.UpdatePlayTimeAsync(dto.UserId, dto.GameId, dto.MinutesPlayed);
                return NoContent();
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error updating play time for user {UserId} and game {GameId}", dto.UserId, dto.GameId);
                return BadRequest(ex.Message);
            }
        }
    }

    public record UpdatePlayTimeDto(string UserId, string GameId, int MinutesPlayed);
}
