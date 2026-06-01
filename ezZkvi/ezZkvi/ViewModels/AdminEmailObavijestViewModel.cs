using System.ComponentModel.DataAnnotations;

namespace ezZkvi.ViewModels
{
    public class AdminEmailObavijestViewModel
    {
        [Required(ErrorMessage = "Grupa primalaca je obavezna.")]
        public string Primaoci { get; set; } = "Studenti";

        [Required(ErrorMessage = "Naslov je obavezan.")]
        public string Naslov { get; set; } = string.Empty;

        [Required(ErrorMessage = "Poruka je obavezna.")]
        public string Poruka { get; set; } = string.Empty;
    }
}