using System.ComponentModel.DataAnnotations;

namespace ezZkvi.Models
{
    public class Feedback
    {
        [Key]
        public int ID { get; set; }

        [EnumDataType(typeof(StatusFeedbacka), ErrorMessage = "Neispravna vrijednost statusa.")]
        public StatusFeedbacka Status { get; set; }

        [EnumDataType(typeof(TipFeedbacka), ErrorMessage = "Neispravna vrijednost tipa feedbacka.")]
        public TipFeedbacka TipFeedbacka { get; set; }

        [Required]
        public DateTime DatumSlanja { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Sadržaj feedbacka je obavezan.")]
        [StringLength(1000, MinimumLength = 10)]
        public string Sadrzaj { get; set; }
    }
}