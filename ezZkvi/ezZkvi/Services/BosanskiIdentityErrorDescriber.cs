using Microsoft.AspNetCore.Identity;

namespace ezZkvi.Services
{
    // Prevodi standardne ASP.NET Identity poruke o greškama na bosanski
    public class BosanskiIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError()
            => new() { Code = nameof(DefaultError), Description = "Došlo je do nepoznate greške." };

        public override IdentityError PasswordTooShort(int length)
            => new() { Code = nameof(PasswordTooShort), Description = $"Lozinka mora imati najmanje {length} karaktera." };

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Lozinka mora sadržavati barem jedan specijalni znak (npr. !, ?, #)." };

        public override IdentityError PasswordRequiresDigit()
            => new() { Code = nameof(PasswordRequiresDigit), Description = "Lozinka mora sadržavati barem jedan broj (0-9)." };

        public override IdentityError PasswordRequiresLower()
            => new() { Code = nameof(PasswordRequiresLower), Description = "Lozinka mora sadržavati barem jedno malo slovo (a-z)." };

        public override IdentityError PasswordRequiresUpper()
            => new() { Code = nameof(PasswordRequiresUpper), Description = "Lozinka mora sadržavati barem jedno veliko slovo (A-Z)." };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
            => new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"Lozinka mora imati najmanje {uniqueChars} različitih karaktera." };

        public override IdentityError PasswordMismatch()
            => new() { Code = nameof(PasswordMismatch), Description = "Pogrešna lozinka." };

        public override IdentityError DuplicateEmail(string email)
            => new() { Code = nameof(DuplicateEmail), Description = $"Email '{email}' je već registrovan." };

        public override IdentityError DuplicateUserName(string userName)
            => new() { Code = nameof(DuplicateUserName), Description = $"Korisničko ime '{userName}' je već zauzeto." };

        public override IdentityError InvalidEmail(string? email)
            => new() { Code = nameof(InvalidEmail), Description = "Email adresa nije ispravna." };

        public override IdentityError InvalidUserName(string? userName)
            => new() { Code = nameof(InvalidUserName), Description = "Korisničko ime sadrži nedozvoljene znakove." };

        public override IdentityError InvalidToken()
            => new() { Code = nameof(InvalidToken), Description = "Token nije ispravan ili je istekao." };

        public override IdentityError UserAlreadyHasPassword()
            => new() { Code = nameof(UserAlreadyHasPassword), Description = "Korisnik već ima postavljenu lozinku." };

        public override IdentityError UserAlreadyInRole(string role)
            => new() { Code = nameof(UserAlreadyInRole), Description = $"Korisnik je već u ulozi '{role}'." };

        public override IdentityError ConcurrencyFailure()
            => new() { Code = nameof(ConcurrencyFailure), Description = "Podaci su u međuvremenu izmijenjeni. Pokušaj ponovo." };
    }
}
