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

        [Required]
        public string Lozinka { get; set; }


    }
}
