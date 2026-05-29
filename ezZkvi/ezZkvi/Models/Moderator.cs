using System.ComponentModel.DataAnnotations;

namespace ezZkvi.Models
{
    public class Moderator : Korisnik
    {
        public int BrojOdgovorenihPitanja { get; set; } = 0;
        public int BrojTacnihOdgovora { get; set; } = 0;
    }
}
