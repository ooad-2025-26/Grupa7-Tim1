using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ezZkvi.Models
{
    public class Pitanje
    {
        [Key]
        public int Id { get; set; }
        public string TekstPitanja { get; set; }
        public Tezina Tezina { get; set; }

        [ForeignKey(nameof(Predmet))]
        public int? PredmetId { get; set; }
        public Predmet? Predmet { get; set; }
        public Pitanje() { }
    }
}
