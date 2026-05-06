using System.ComponentModel.DataAnnotations;

namespace ezZkvi.Models
{
    public class KvizSesija
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public int TraziBrojPitanja { get; set; }
        [Required]
        public int VremenskoOgranicenje { get; set; }

        public StatusSesije Status { get; set; }

    }
}
