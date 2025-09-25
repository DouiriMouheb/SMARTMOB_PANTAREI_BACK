using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTMOB_PANTAREI_BACK.Models
{
    [Table("ACQUISIZIONI")]
    public class Acquisizioni
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("COD_LINEA")]
        [StringLength(50)]
        public string? CodLinea { get; set; }

        [Column("COD_POSTAZIONE")]
        [StringLength(50)]
        public string? CodPostazione { get; set; }

        [Column("FOTO_ACQUISIZIONE")]
        [StringLength(255)]
        public string? FotoAcquisizione { get; set; }

        [Column("CODICE_ARTICOLO")]
        [StringLength(50)]
        public string? CodiceArticolo { get; set; }

        [Column("ID_CATASTA")]
        [StringLength(50)]
        public string? IdCatasta { get; set; }

        [Column("ABILITA_CQ")]
        public bool AbilitaCq { get; set; }

        [Column("ESITO_CQ_ARTICOLO")]
        public bool? EsitoCqArticolo { get; set; }

        [Column("NUM_SPINE_CONTATE")]
        public int? NumSpineContate { get; set; }

        [Column("NUM_SPINE_ATTESE")]
        public int? NumSpineAttese { get; set; }

        [Column("DT_INS")]
        public DateTime? DataInserimento { get; set; }

        [Column("DT_AGG")]
        public DateTime? DataAggiornamento { get; set; }

        [Column("DESCRIZIONE")]
        [StringLength(500)]
        public string? Descrizione { get; set; }
    }
}
