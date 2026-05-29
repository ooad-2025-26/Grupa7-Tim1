using System.ComponentModel.DataAnnotations;

namespace ezZkvi.Models
{
    public class Student : Korisnik
    {
        [Range(0, int.MaxValue, ErrorMessage = "Broj odgovorenih pitanja ne moze biti negativan.")]
        public int BrojOdgovorenihPitanja { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Broj tacnih odgovora ne moze biti negativan.")]
        public int BrojTacnihOdgovora { get; set; } = 0;
    }
}