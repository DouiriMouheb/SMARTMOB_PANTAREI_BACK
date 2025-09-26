using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTMOB_PANTAREI_BACK.Models
{
    [Table("vw_PostazioniPerLinea")]
    public class PostazioniPerLinea
    {
        [Key]
        [Column("COD_LINEA")]
        [StringLength(50)]
        public string CodLinea { get; set; } = string.Empty;

        [Column("COD_POSTAZIONE_LIST")]
        public string CodPostazioneList { get; set; } = string.Empty;
    }
}