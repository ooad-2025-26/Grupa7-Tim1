using System.ComponentModel.DataAnnotations;

namespace ezZkvi.Models
{
    public class Moderator : Korisnik
    {
        public int BrojOdgovorenihPitanja { get; set; } = 0;
        public int BrojTacnihOdgovora { get; set; } = 0;

        public virtual ICollection<KvizSesija> KvizSesije { get; set; }
        public virtual ICollection<Feedback> PregledaniFeedbackovi { get; set; }

        public Moderator()
        {
            KvizSesije = new List<KvizSesija>();
            PregledaniFeedbackovi = new List<Feedback>();
        }
    }
}
