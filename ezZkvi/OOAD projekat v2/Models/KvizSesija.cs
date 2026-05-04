using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
