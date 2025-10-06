namespace SMARTMOB_PANTAREI_BACK.DTOs
{
    /// <summary>
    /// Request model for forwarding image to Quality Control API
    /// </summary>
    public class ForwardImageRequest
    {
        /// <summary>
        /// The filename of the image to forward (should exist in Public folder)
        /// </summary>
        public string Filename { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model for Quality Control operations
    /// </summary>
    public class QualityControlResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }
}
