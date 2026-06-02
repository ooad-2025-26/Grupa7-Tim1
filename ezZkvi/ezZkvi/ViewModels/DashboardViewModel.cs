namespace ezZkvi.ViewModels
{
    public class DashboardViewModel
    {
        // Statistika (kartice na vrhu)
        public int KvizoviZavrseni { get; set; }
        public int ProsjecanRezultat { get; set; }   // u procentima
        public int? Rang { get; set; }
        public int UkupnoStudenata { get; set; }

        // Nivo / XP
        public int Nivo { get; set; }
        public int XpUkupno { get; set; }
        public int XpUNivou { get; set; }            // bodovi unutar trenutnog nivoa (0-99)
        public int XpZaNivo { get; set; } = 100;     // koliko XP treba za sljedeći nivo

        public List<NedavnaAktivnostViewModel> NedavneAktivnosti { get; set; } = new();
        public List<PredmetNapredakViewModel> MojiPredmeti { get; set; } = new();
    }

    public class NedavnaAktivnostViewModel
    {
        public string PredmetNaziv { get; set; } = "Predmet";
        public int Procenat { get; set; }
        public DateTime Datum { get; set; }
    }

    public class PredmetNapredakViewModel
    {
        public string Naziv { get; set; } = "Predmet";
        public int Procenat { get; set; }
        public int BrojPitanja { get; set; }
    }
}
