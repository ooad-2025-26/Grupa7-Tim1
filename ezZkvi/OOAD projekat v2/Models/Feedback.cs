using System.ComponentModel.DataAnnotations;

namespace ezZkvi.Models
{
    public class Feedback
    {
        [Key]
        public int ID { get; set; }

        public StatusFeedbacka Status { get; set; }

        public TipFeedbacka TipFeedbacka { get; set; }

        [Required]
        public DateTime DatumSlanja { get; set; }
    }
}