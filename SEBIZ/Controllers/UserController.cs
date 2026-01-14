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
        private readonly ITransactionService _transactionService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ITransactionService transactionService, ILogger<UserController> logger)
        {
            _userService = userService;
            _transactionService = transactionService;
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

        [Authorize]
        [HttpPost("add-balance")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddBalance([FromBody] AddBalanceDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                await _userService.AddBalanceAsync(userId, dto.Amount);
                return NoContent();
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error adding balance to user");
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var user = await _userService.GetMeAsync(userId);
                return Ok(user);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error getting user");
                return NotFound(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("my-library")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetOwnedGames()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var games = await _userService.GetOwnedGamesAsync(userId);
                return Ok(games);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error getting owned games");
                return NotFound(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("transactions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactions()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var transactions = await _transactionService.GetUserTransactionsAsync(userId);
                return Ok(transactions);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error getting user transactions");
                return BadRequest(ex.Message);
            }
        }

        // Admin endpoints
        [Authorize]
        [HttpGet("admin/all-transactions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAllTransactions()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var user = await _userService.GetMeAsync(userId);
                if (user.Role != "Admin")
                {
                    return Forbid();
                }

                var transactions = await _transactionService.GetAllTransactionsAsync();
                return Ok(transactions);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error getting all transactions");
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("admin/all-users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var user = await _userService.GetMeAsync(userId);
                if (user.Role != "Admin")
                {
                    return Forbid();
                }

                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("admin/promote/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> PromoteToAdmin(string userId)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized();
                }

                var admin = await _userService.GetMeAsync(adminId);
                if (admin.Role != "Admin")
                {
                    return Forbid();
                }

                var promotedUser = await _userService.PromoteToAdminAsync(userId);
                return Ok(promotedUser);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error promoting user to admin");
                return ex.Message.Contains("not found") ? NotFound(ex.Message) : BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("admin/demote/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> DemoteAdmin(string userId)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized();
                }

                var admin = await _userService.GetMeAsync(adminId);
                if (admin.Role != "Admin")
                {
                    return Forbid();
                }

                var demotedUser = await _userService.DemoteAdminAsync(userId);
                return Ok(demotedUser);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error demoting admin");
                return ex.Message.Contains("not found") ? NotFound(ex.Message) : BadRequest(ex.Message);
            }
        }
    }
}

