using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Service;
using System.Threading.Tasks;

namespace SEBIZ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterUserDto dto)
        {
            try
            {
                var user = await _userService.RegisterAsync(dto);
                return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error registering user");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginUserDto dto)
        {
            try
            {
                var user = await _userService.LoginAsync(dto);
                return Ok(user);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error logging in user");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{userId}/library/{gameId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddGameToLibrary(string userId, string gameId)
        {
            try
            {
                await _userService.AddGameToUserLibraryAsync(userId, gameId);
                return NoContent();
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error adding game {GameId} to user {UserId}", gameId, userId);
                return ex.Message.Contains("not found") ? NotFound(ex.Message) : BadRequest(ex.Message);
            }
        }

        [HttpGet("{userId}/playtime")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> GetTotalPlayTime(string userId)
        {
            try
            {
                var playtime = await _userService.GetTotalPlayTime(userId);
                return Ok(playtime);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error getting playtime for user {UserId}", userId);
                return NotFound(ex.Message);
            }
        }
    }
}
