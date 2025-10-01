namespace SMARTMOB_PANTAREI_BACK.DTOs
{
    public class AcquisizioniDto
    {
        public int Id { get; set; }
        public string? CodLinea { get; set; }
        public string? CodPostazione { get; set; }
        public string? FotoAcquisizione { get; set; }
        public string? CodiceArticolo { get; set; }
        public string? IdCatasta { get; set; }
        public bool AbilitaCq { get; set; }
    public bool? EsitoCqArticolo { get; set; }
        public int? NumSpineContate { get; set; }
        public int? NumSpineAttese { get; set; }
        public DateTime? DataInserimento { get; set; }
        public DateTime? DataAggiornamento { get; set; }
        public string? Descrizione { get; set; }
    }
}