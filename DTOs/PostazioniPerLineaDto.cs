using System.Text.Json.Serialization;

namespace SMARTMOB_PANTAREI_BACK.DTOs
{
    public class PostazioniPerLineaDto
    {
        [JsonPropertyName("coD_LINEA_PROD")]
        public string CoD_LINEA_PROD { get; set; } = string.Empty;

        [JsonPropertyName("coD_POSTAZIONE")]
        public List<string> CoD_POSTAZIONE { get; set; } = new List<string>();
    }
}