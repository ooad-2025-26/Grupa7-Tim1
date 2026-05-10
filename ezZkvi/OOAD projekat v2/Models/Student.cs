using System.ComponentModel.DataAnnotations;

namespace ezZkvi.Models
{
    public class Student : Korisnik
    {
        public int BrojOdgovorenihPitanja { get; set; } = 0;
        public int BrojTacnihOdgovora { get; set; } = 0;
    }
}
