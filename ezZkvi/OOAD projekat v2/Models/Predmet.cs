using System.ComponentModel.DataAnnotations;

namespace ezZkvi.Models
{
    public class Predmet
    {
        [Key]
        public int Id { get; set; }
        public string Naziv { get; set; }
        public Predmet() { }
    }
}
