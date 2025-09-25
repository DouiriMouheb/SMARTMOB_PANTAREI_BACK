namespace SMARTMOB_PANTAREI_BACK.DTOs
{
    public class PostazioniDto
    {
        public string CodLineaProd { get; set; } = string.Empty;
        public string CodPostazione { get; set; } = string.Empty;
        // public bool FlgRullieraBox { get; set; }
        public DateTime? DataInserimento { get; set; }
    }

    public class CreatePostazioniDto
    {
        public string CodLineaProd { get; set; } = string.Empty;
        public string CodPostazione { get; set; } = string.Empty;
        // public bool FlgRullieraBox { get; set; }
    }

    public class UpdatePostazioniDto
    {
        // public bool FlgRullieraBox { get; set; }
    }
}