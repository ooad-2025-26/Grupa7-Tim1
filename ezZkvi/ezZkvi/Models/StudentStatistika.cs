namespace ezZkvi.Models
{
    public class StudentStatistika
    {
        public int Id { get; set; }

        public string KorisnikId { get; set; } = string.Empty;

        public int PredmetId { get; set; }

        public int BrojKvizova { get; set; }

        public int UkupnoPitanja { get; set; }

        public int TacniOdgovori { get; set; }
    }
}
