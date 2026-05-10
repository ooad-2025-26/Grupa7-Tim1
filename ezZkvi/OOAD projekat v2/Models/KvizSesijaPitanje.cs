using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ezZkvi.Models
{   
    public class KvizSesijaPitanje
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey(nameof(KvizSesija))]
        public int RedniBroj { get; set; }

        [Required]
        public double BrojBodova { get; set; }
            
        [Required]
        public double Tacno { get; set; }
    }
}
