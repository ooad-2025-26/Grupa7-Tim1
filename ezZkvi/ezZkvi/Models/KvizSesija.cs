using System.ComponentModel.DataAnnotations;

namespace ezZkvi.Models
{
    public class KvizSesija
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Broj pitanja mora biti veći od 0.")]
        public int TraziBrojPitanja { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Vremensko ograničenje mora biti veće od 0.")]
        public int VremenskoOgranicenje { get; set; }

        [EnumDataType(typeof(StatusSesije), ErrorMessage = "Neispravna vrijednost statusa.")]
        public StatusSesije Status { get; set; }
    }
}