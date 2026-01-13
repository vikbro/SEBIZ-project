using Microsoft.AspNetCore.Mvc;
using SEBIZ.Service;
using System.Threading.Tasks;
using SEBIZ.Domain.Models;

namespace SEBIZ.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameUsageController : ControllerBase
    {
        private readonly IGameUsageService _gameUsageService;

        public GameUsageController(IGameUsageService gameUsageService)
        {
            _gameUsageService = gameUsageService;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdatePlayTime([FromBody] GameUsage gameUsage)
        {
            await _gameUsageService.UpdatePlayTime(gameUsage);
            return Ok();
        }
    }
}
