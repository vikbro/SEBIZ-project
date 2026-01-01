using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SEBIZ.Domain.Contracts;
using SEBIZ.Service;
using System.Security.Claims;
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

        [Authorize]
        [HttpPost("purchase/{gameId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PurchaseGame(string gameId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
                await _userService.PurchaseGameAsync(userId, gameId);
                return NoContent();
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error adding game {GameId} to user", gameId);
                return ex.Message.Contains("not found") ? NotFound(ex.Message) : BadRequest(ex.Message);
            }
        }
    }
}
