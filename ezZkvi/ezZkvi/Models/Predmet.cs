using System.ComponentModel.DataAnnotations;

namespace ezZkvi.Models
{
    public class Predmet
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv predmeta je obavezan.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Naziv mora imati između 2 i 100 karaktera.")]
        public string Naziv { get; set; }

        public Predmet() { }
    }
}