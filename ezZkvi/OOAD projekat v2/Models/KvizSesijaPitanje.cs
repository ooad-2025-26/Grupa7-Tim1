using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ezZkvi.Models
{
    public class KvizSesijaPitanje
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey(nameof(KvizSesija))]
        [Range(1, int.MaxValue, ErrorMessage = "Redni broj mora biti veći od 0.")]
        public int RedniBroj { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Broj bodova ne može biti negativan.")]
        public double BrojBodova { get; set; }

        [Required]
        [Range(0, 1, ErrorMessage = "Tacno mora biti između 0 i 1.")]
        public double Tacno { get; set; }
    }
}