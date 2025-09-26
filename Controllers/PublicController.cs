using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace SMARTMOB_PANTAREI_BACK.Controllers
{
    [ApiController]
    [Route("api/public")]
    public class PublicController : ControllerBase
    {
        private readonly string _publicFolder;
        public PublicController(IWebHostEnvironment env)
        {
            _publicFolder = Path.Combine(env.ContentRootPath, "Public");
        }

        [HttpGet("{imageName}")]
        public IActionResult GetImage(string imageName)
        {
            var filePath = Path.Combine(_publicFolder, imageName);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = GetContentType(imageName);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType);
        }

        private string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
