using System.ComponentModel.DataAnnotations;

namespace ezZkvi.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email je obavezan")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Lozinka je obavezna")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}