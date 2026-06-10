using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ezZkvi.Models
{
    public class Oblast
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv oblasti je obavezan.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Naziv oblasti mora imati između 2 i 100 karaktera.")]
        public string Naziv { get; set; } = string.Empty;

        [ForeignKey(nameof(Predmet))]
        public int PredmetId { get; set; }
        public Predmet? Predmet { get; set; }

        public ICollection<Pitanje> Pitanja { get; set; } = new List<Pitanje>();
    }
}
