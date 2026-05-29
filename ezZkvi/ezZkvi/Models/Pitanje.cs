using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ezZkvi.Models
{
    public class Pitanje
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tekst pitanja je obavezan.")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Tekst pitanja mora imati između 5 i 1000 karaktera.")]
        public string TekstPitanja { get; set; }

        [EnumDataType(typeof(Tezina), ErrorMessage = "Neispravna vrijednost težine.")]
        public Tezina Tezina { get; set; }

        [ForeignKey(nameof(Predmet))]
        public int? PredmetId { get; set; }
        public Predmet? Predmet { get; set; }

        public Pitanje() { }
    }
}