using System.Text.RegularExpressions;

namespace ezZkvi.Services
{
    public static class ContentValidation
    {
        private static readonly Regex DozvoljeniNazivRegex = new(@"^[\p{L}\p{N}\s]+$", RegexOptions.Compiled);
        private static readonly Regex ViseRazmakaRegex = new(@"\s+", RegexOptions.Compiled);

        public static string NormalizujUnos(string? vrijednost)
        {
            return ViseRazmakaRegex.Replace((vrijednost ?? string.Empty).Trim(), " ");
        }

        public static bool NazivImaDozvoljeneZnakove(string naziv)
        {
            return DozvoljeniNazivRegex.IsMatch(naziv);
        }

        public static string KljucZaPoredjenje(string vrijednost)
        {
            return NormalizujUnos(vrijednost).ToLower();
        }
    }
}
