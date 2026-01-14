using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace SEBIZ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameFileController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GameFileController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadGameFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Ok(new { GameFilePath = (string?)null });
            }

            var maxFileSize = 100 * 1024 * 1024; // 100MB
            if (file.Length > maxFileSize)
            {
                return BadRequest("File size exceeds the limit of 100MB.");
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (fileExtension != ".zip")
            {
                return BadRequest("Invalid file format. Only .zip files are allowed.");
            }

            var gamesFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "games");
            if (!Directory.Exists(gamesFolderPath))
            {
                Directory.CreateDirectory(gamesFolderPath);
            }

            // Create a unique folder for this game
            var folderName = Guid.NewGuid().ToString();
            var gameFolderPath = Path.Combine(gamesFolderPath, folderName);
            Directory.CreateDirectory(gameFolderPath);

            try
            {
                // Save the zip file temporarily
                var tempZipPath = Path.Combine(gameFolderPath, "temp.zip");
                using (var stream = new FileStream(tempZipPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Extract the zip file
                ZipFile.ExtractToDirectory(tempZipPath, gameFolderPath, overwriteFiles: true);

                // Delete the temporary zip file
                System.IO.File.Delete(tempZipPath);

                // Return path to the extracted folder
                var relativePath = $"/games/{folderName}";
                return Ok(new { GameFilePath = relativePath, FileName = folderName });
            }
            catch (Exception ex)
            {
                // Clean up on error
                if (Directory.Exists(gameFolderPath))
                {
                    Directory.Delete(gameFolderPath, recursive: true);
                }
                return BadRequest($"File upload/extraction failed: {ex.Message}");
            }
        }

        [HttpDelete("delete")]
        public IActionResult DeleteGameFile([FromQuery] string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            try
            {
                var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "games", fileName);
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, recursive: true);
                    return Ok(new { Message = "Game file deleted successfully." });
                }
                else
                {
                    return NotFound("Game folder not found.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"File deletion failed: {ex.Message}");
            }
        }

        [HttpDelete("image-delete")]
        public IActionResult DeleteImageFile([FromQuery] string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            try
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Ok(new { Message = "Image file deleted successfully." });
                }
                else
                {
                    return NotFound("Image file not found.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"File deletion failed: {ex.Message}");
            }
        }

        [HttpGet("serve/{gameFolderId}/{*filePath}")]
        public IActionResult ServeGameFile(string gameFolderId, string filePath = "")
        {
            try
            {
                var gameFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "games", gameFolderId);
                
                if (!Directory.Exists(gameFolderPath))
                {
                    return NotFound($"Game folder not found: {gameFolderId}");
                }

                // Find index.html and get its directory
                string indexHtmlPath = FindFile(gameFolderPath, "index.html");
                string indexHtmlDirectory;

                if (indexHtmlPath == null)
                {
                    var contents = string.Join(", ", Directory.GetFiles(gameFolderPath, "*", SearchOption.AllDirectories));
                    return NotFound($"index.html not found in {gameFolderId}. Contents: {contents}");
                }

                indexHtmlDirectory = Path.GetDirectoryName(indexHtmlPath);

                // If no file specified, serve index.html
                if (string.IsNullOrEmpty(filePath))
                {
                    return ServeHtmlWithBaseHref(indexHtmlPath, gameFolderId, indexHtmlDirectory, gameFolderPath);
                }

                // First, try to find the file relative to the index.html directory (for nested games)
                var fullFilePath = Path.Combine(indexHtmlDirectory, filePath);
                
                // If file not found there, try relative to the game folder root
                if (!System.IO.File.Exists(fullFilePath))
                {
                    fullFilePath = Path.Combine(gameFolderPath, filePath);
                }
                
                // Security check - ensure the file is within the game folder
                var normalizedGamePath = Path.GetFullPath(gameFolderPath);
                var normalizedFilePath = Path.GetFullPath(fullFilePath);
                
                if (!normalizedFilePath.StartsWith(normalizedGamePath))
                {
                    return Forbid("Invalid file path");
                }

                if (!System.IO.File.Exists(fullFilePath))
                {
                    return NotFound($"File not found: {filePath}");
                }

                // Get file extension
                var extension = Path.GetExtension(fullFilePath).ToLowerInvariant();

                // If it's an HTML file, inject base href
                if (extension == ".html")
                {
                    return ServeHtmlWithBaseHref(fullFilePath, gameFolderId, indexHtmlDirectory, gameFolderPath);
                }

                // For other files, serve normally
                var contentType = GetContentType(fullFilePath);
                var fileContent = System.IO.File.ReadAllBytes(fullFilePath);
                
                return File(fileContent, contentType);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error serving game: {ex.Message}");
            }
        }

        private IActionResult ServeHtmlWithBaseHref(string htmlFilePath, string gameFolderId, string indexHtmlDirectory, string gameFolderPath)
        {
            var htmlContent = System.IO.File.ReadAllText(htmlFilePath);
            
            // The base href should always point to the game folder root through the serve endpoint
            // All file paths are resolved relative to the game folder, not the index.html directory
            var baseHref = $"/api/GameFile/serve/{gameFolderId}/";
            
            // Inject base tag if not already present
            if (!htmlContent.Contains("<base"))
            {
                var baseTag = $"<base href=\"{baseHref}\">";
                
                // Insert after <head> tag
                if (htmlContent.Contains("<head>"))
                {
                    htmlContent = htmlContent.Replace("<head>", $"<head>\n    {baseTag}");
                }
                else if (htmlContent.Contains("<HEAD>"))
                {
                    htmlContent = htmlContent.Replace("<HEAD>", $"<HEAD>\n    {baseTag}");
                }
                else
                {
                    // If no head tag, add before body
                    htmlContent = htmlContent.Replace("<body", $"{baseTag}\n<body");
                }
            }
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(htmlContent);
            return File(bytes, "text/html");
        }

        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".wasm" => "application/wasm",
                ".woff" => "font/woff",
                ".woff2" => "font/woff2",
                ".ttf" => "font/ttf",
                _ => "application/octet-stream"
            };
        }

        private string FindFile(string directory, string fileName)
        {
            try
            {
                var files = Directory.GetFiles(directory, fileName, SearchOption.AllDirectories);
                return files.Length > 0 ? files[0] : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
