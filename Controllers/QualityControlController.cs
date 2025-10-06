using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using SMARTMOB_PANTAREI_BACK.DTOs;

namespace SMARTMOB_PANTAREI_BACK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QualityControlController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<QualityControlController> _logger;
        private readonly IConfiguration _configuration;

        public QualityControlController(
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment environment,
            ILogger<QualityControlController> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _environment = environment;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("forward")]
        public async Task<IActionResult> ForwardImageToQualityControl([FromBody] ForwardImageRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Filename))
                {
                    return BadRequest(new { error = "Filename is required" });
                }

                _logger.LogInformation($"[QualityControl] Forwarding image: {request.Filename}");

                // Step 1: Get the image from local storage (Public folder)
                var imagePath = Path.Combine(_environment.ContentRootPath, "Public", request.Filename);
                
                if (!System.IO.File.Exists(imagePath))
                {
                    _logger.LogWarning($"[QualityControl] Image not found: {imagePath}");
                    return NotFound(new { error = $"Image not found: {request.Filename}" });
                }

                byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
                _logger.LogInformation($"[QualityControl] Image loaded: {imageBytes.Length} bytes");

                // Step 2: Create multipart form data
                using var content = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(imageBytes);
                
                // Set proper content type based on file extension
                var extension = Path.GetExtension(request.Filename).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".bmp" => "image/bmp",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };
                
                imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                content.Add(imageContent, "image", request.Filename);

                // Step 3: POST to QualityControlVisual API
                var qualityControlUrl = _configuration["QualityControlApi:Url"] 
                    ?? "http://192.168.1.118:8000/QualityControlVisual";
                
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMinutes(2); // Adjust timeout as needed

                _logger.LogInformation($"[QualityControl] Posting to: {qualityControlUrl}");
                
                var response = await httpClient.PostAsync(qualityControlUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"[QualityControl] API error: {response.StatusCode} - {errorContent}");
                    return StatusCode((int)response.StatusCode, 
                        new { error = $"QualityControl API failed: {response.StatusCode}", details = errorContent });
                }

                // Step 4: Return the processed image to frontend
                var resultBytes = await response.Content.ReadAsByteArrayAsync();
                var resultContentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                
                _logger.LogInformation($"[QualityControl] ✓ Success: Returning {resultBytes.Length} bytes, Type: {resultContentType}");

                return File(resultBytes, resultContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[QualityControl] ✗ Error processing request");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
}
