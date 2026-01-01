using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Service;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SEBIZ.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly IRecommendationService _recommendationService;
        private readonly ILogger<GameController> _logger;


        public GameController(IGameService gameService, IRecommendationService recommendationService, ILogger<GameController> logger)
        {
            _gameService = gameService;
            _recommendationService = recommendationService;
            _logger = logger;
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GameDto>> CreateGame([FromBody] CreateGameDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
                var game = await _gameService.CreateGameAsync(dto, userId);
                return CreatedAtAction(nameof(GetGameById), new { id = game.Id }, game);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error creating game");
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameDto>> GetGameById([FromRoute] string id)
        {
            try
            {
                var game = await _gameService.GetGameByIdAsync(id);
                return Ok(game);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error retrieving game {GameId}", id);
                return ex.Message.Contains("not found") ? NotFound(ex.Message) : BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetAllGames()
        {
            try
            {
                var games = await _gameService.GetAllGamesAsync();
                return Ok(games);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error retrieving all games");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GameDto>> UpdateGame([FromRoute] string id, [FromBody] UpdateGameDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
                var game = await _gameService.UpdateGameAsync(id, dto, userId);
                return Ok(game);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error updating game {GameId}", id);
                return ex.Message.Contains("not found") ? NotFound(ex.Message) : BadRequest(ex.Message);
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteGame([FromRoute] string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
                await _gameService.DeleteGameAsync(id, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error deleting game {GameId}", id);
                return ex.Message.Contains("not found") ? NotFound(ex.Message) : BadRequest(ex.Message);
            }
        }

        [HttpGet("recommendations/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetRecommendations(string userId)
        {
            try
            {
                var recommendations = await _recommendationService.GetRecommendationsAsync(userId);
                return Ok(recommendations);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error getting recommendations for user {UserId}", userId);
                return BadRequest(ex.Message);
            }
        }
    }
}
