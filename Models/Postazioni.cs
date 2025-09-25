using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMARTMOB_PANTAREI_BACK.Models
{
    [Table("AG_POSTAZIONI")]
    public class Postazioni
    {
        [Key]
        [Column("COD_LINEA_PROD")]
        [StringLength(50)]
        public string CodLineaProd { get; set; } = string.Empty;

        [Key]
        [Column("COD_POSTAZIONE")]
        [StringLength(50)]
        public string CodPostazione { get; set; } = string.Empty;

        // Temporarily commented out to test
        // public bool FlgRullieraBox { get; set; }

        public DateTime? DataInserimento { get; set; }
    }
}