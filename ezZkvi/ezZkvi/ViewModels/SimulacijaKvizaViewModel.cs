using ezZkvi.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ezZkvi.ViewModels
{
    public class SimulacijaKvizaViewModel
    {
        public int? PredmetId { get; set; }
        public string? PredmetNaziv { get; set; }

        public int BrojPitanja { get; set; } = 10;
        public int VremenskoOgranicenjeMinuta { get; set; } = 15;

        public long StartedAtUtcTicks { get; set; }

        public string? ErrorMessage { get; set; }

        public List<SelectListItem> Predmeti { get; set; } = new();
        public List<SimulacijaPitanjeViewModel> Questions { get; set; } = new();

        public SimulacijaRezultatViewModel? Result { get; set; }
    }

    public class SimulacijaPitanjeViewModel
    {
        public int Id { get; set; }
        public string TekstPitanja { get; set; } = string.Empty;
        public Tezina Tezina { get; set; }

        public List<SimulacijaOdgovorViewModel> Odgovori { get; set; } = new();
    }

    public class SimulacijaOdgovorViewModel
    {
        public int Id { get; set; }
        public string Tekst { get; set; } = string.Empty;
    }

    public class SimulacijaSubmitViewModel
    {
        public int PredmetId { get; set; }
        public long StartedAtUtcTicks { get; set; }
        public int TotalSeconds { get; set; }

        public List<SimulacijaKorisnickiOdgovorViewModel> Odgovori { get; set; } = new();
    }

    public class SimulacijaKorisnickiOdgovorViewModel
    {
        public int PitanjeId { get; set; }
        public int? OdgovorId { get; set; }
    }

    public class SimulacijaRezultatViewModel
    {
        public int UkupnoPitanja { get; set; }
        public int TacnihOdgovora { get; set; }
        public int NetacnihOdgovora { get; set; }
        public int Neodgovorenih { get; set; }
        public int Procenat { get; set; }
        public int UtrosenoSekundi { get; set; }

        public List<SimulacijaPregledOdgovoraViewModel> Pregled { get; set; } = new();
    }

    public class SimulacijaPregledOdgovoraViewModel
    {
        public string TekstPitanja { get; set; } = string.Empty;
        public string? KorisnickiOdgovor { get; set; }
        public string TacanOdgovor { get; set; } = string.Empty;

        public bool JeTacno { get; set; }
        public bool JeOdgovoreno { get; set; }
    }
}