using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ezZkvi.Models
{
    public class Odgovor
    {
        [Key]
        public int Id { get; set; } 
        public string Tekst { get; set; }
        public bool IsTacan { get; set; }

        [ForeignKey(nameof(Pitanje))]
        public int? PitanjeId { get; set; }
        public Pitanje? Pitanje { get; set; }
        public Odgovor() { }

    }
}
