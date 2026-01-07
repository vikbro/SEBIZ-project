using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SEBIZ.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>
        {
            {".png", "image/png"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
        };

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (file.Length > 5 * 1024 * 1024) // 5MB limit
                return BadRequest("File size exceeds the 5MB limit.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".png")
                return BadRequest("Invalid file type. Only .jpg and .png are allowed.");

            var fileName = $"{Guid.NewGuid()}{extension}";
            var directoryPath = Path.Combine("Images", "Games");
            Directory.CreateDirectory(directoryPath); // Create directory if it doesn't exist
            var path = Path.Combine(directoryPath, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { imagePath = path });
        }

        [HttpGet("{imageName}")]
        public IActionResult GetImage(string imageName)
        {
            // Sanitize imageName to prevent path traversal
            var fileName = Path.GetFileName(imageName);
            var path = Path.Combine("Images", "Games", fileName);

            if (!System.IO.File.Exists(path))
                return NotFound();

            var image = System.IO.File.OpenRead(path);
            var contentType = GetContentType(path);
            return File(image, contentType);
        }

        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (MimeTypes.TryGetValue(ext, out var contentType))
            {
                return contentType;
            }
            return "application/octet-stream";
        }
    }
}
