using Microsoft.AspNetCore.Identity;

namespace ezZkvi.Models
{
    public class Korisnik : IdentityUser
    {
        public bool IsApproved { get; set; } = false;

        public DateTime? LastActivity { get; set; }
    }
}