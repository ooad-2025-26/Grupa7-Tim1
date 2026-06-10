using System.ComponentModel.DataAnnotations;
using ezZkvi.Models;

namespace ezZkvi.ViewModels
{
    public class PitanjeSaOdgovorimaViewModel
    {
        [Required(ErrorMessage = "Tekst pitanja je obavezan.")]
        public string TekstPitanja { get; set; } = string.Empty;

        [Required(ErrorMessage = "Predmet je obavezan.")]
        public int PredmetId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Oblast je obavezna.")]
        public int OblastId { get; set; }

        public Tezina Tezina { get; set; }

        [Required(ErrorMessage = "Odgovor 1 je obavezan.")]
        public string Odgovor1 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Odgovor 2 je obavezan.")]
        public string Odgovor2 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Odgovor 3 je obavezan.")]
        public string Odgovor3 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Odgovor 4 je obavezan.")]
        public string Odgovor4 { get; set; } = string.Empty;

        [Range(1, 4, ErrorMessage = "Mora biti odabran tačan odgovor.")]
        public int TacanOdgovor { get; set; }
    }
}
