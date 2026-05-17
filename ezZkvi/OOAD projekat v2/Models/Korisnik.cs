using System.ComponentModel.DataAnnotations;

namespace ezZkvi.Models
{
    public class Korisnik
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Primjer: Lozinka mora imati barem jedno veliko slovo, jedno malo slovo i broj
        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Lozinka mora imati najmanje 8 karaktera.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Lozinka mora sadržavati barem jedno veliko slovo, jedno malo slovo i jedan broj.")]
        public string Lozinka { get; set; }


    }
}
