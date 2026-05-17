using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ezZkvi.Models
{
    public class Odgovor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tekst odgovora je obavezan.")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Tekst odgovora mora imati između 1 i 500 karaktera.")]
        public string Tekst { get; set; }

        public bool IsTacan { get; set; }

        [ForeignKey(nameof(Pitanje))]
        public int? PitanjeId { get; set; }
        public Pitanje? Pitanje { get; set; }

        public Odgovor() { }
    }
}