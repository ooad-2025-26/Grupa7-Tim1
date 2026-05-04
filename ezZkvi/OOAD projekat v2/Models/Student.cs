using System.ComponentModel.DataAnnotations;

namespace ezZkvi.Models
{
    public class Student : Korisnik
    {
        public int BrojOdgovorenihPitanja { get; set; } = 0;
        public int BrojTacnihOdgovora { get; set; } = 0;

        
        public virtual ICollection<KvizSesija> KvizSesije { get; set; }
        public virtual ICollection<Feedback> PoslaniFeedbackovi { get; set; }

        public Student()
        {
            KvizSesije = new List<KvizSesija>();
            PoslaniFeedbackovi = new List<Feedback>();
        }
    }
}
