using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts.MogoDbProductAPI.Domain.Contracts;
using SEBIZ.Service;

namespace SEBIZ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly IRecommendationService _recommendationService;
        private readonly ILogger<GameController> _logger;
        private readonly IPlaytimeService _playtimeService;


        public GameController(IGameService gameService, IRecommendationService recommendationService, ILogger<GameController> logger, IPlaytimeService playtimeService)
        {
            _gameService = gameService;
            _recommendationService = recommendationService;
            _logger = logger;
            _playtimeService = playtimeService;
        }


        [HttpPost("playtime/start")]
        public async Task<IActionResult> StartPlaytime([FromBody] PlaytimeRequest request)
        {
            await _playtimeService.StartPlaytimeAsync(request.UserId, request.GameId);
            return Ok();
        }

        [HttpPost("playtime/stop")]
        public async Task<IActionResult> StopPlaytime([FromBody] PlaytimeRequest request)
        {
            await _playtimeService.StopPlaytimeAsync(request.UserId, request.GameId);
            return Ok();
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GameDto>> CreateGame([FromBody] CreateGameDto dto)
        {
            try
            {
                var game = await _gameService.CreateGameAsync(dto);
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
        public async Task<ActionResult<GameDto>> UpdateGame([FromRoute] string id, [FromBody] UpdateGameDto dto)
        {
            try
            {
                var game = await _gameService.UpdateGameAsync(id, dto);
                return Ok(game);
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
        public async Task<IActionResult> DeleteGame([FromRoute] string id)
        {
            try
            {
                await _gameService.DeleteGameAsync(id);
                return NoContent();
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
