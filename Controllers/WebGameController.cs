using Microsoft.AspNetCore.Mvc;

namespace SEBIZ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebGameController : ControllerBase
    {
        [HttpGet("godot")]
        public IActionResult PlayGodotGame()
        {
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "SEBIZ", "wwwroot", "games", "GodotGame", "index.html"), "text/html");
        }
    }
}
