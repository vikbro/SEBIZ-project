using Microsoft.AspNetCore.Mvc;
using SEBIZ.Service;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SEBIZ.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameUploadController : ControllerBase
    {
        private readonly IGameUploadService _gameUploadService;

        public GameUploadController(IGameUploadService gameUploadService)
        {
            _gameUploadService = gameUploadService;
        }

        [HttpPost("upload/{gameId}")]
        public async Task<IActionResult> UploadGame(string gameId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (System.IO.Path.GetExtension(file.FileName).ToLower() != ".zip")
            {
                return BadRequest("Only .zip files are allowed.");
            }

            try
            {
                var gamePath = await _gameUploadService.UploadGame(gameId, file);
                return Ok(new { gamePath });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
