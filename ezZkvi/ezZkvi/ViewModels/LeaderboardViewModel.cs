namespace ezZkvi.ViewModels
{
    public class LeaderboardViewModel
    {
        public List<LeaderboardEntryViewModel> Entries { get; set; } = new();
    }

    public class LeaderboardEntryViewModel
    {
        public string Ime { get; set; } = "Student";
        public string Inicijali { get; set; } = "ST";
        public int Bodovi { get; set; }
        public int Tacnost { get; set; }   // u procentima
        public int Kvizovi { get; set; }
        public bool JeTrenutni { get; set; }
    }
}
