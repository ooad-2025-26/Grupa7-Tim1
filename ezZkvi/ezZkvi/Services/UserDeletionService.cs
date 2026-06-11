using ezZkvi.Data;
using Microsoft.EntityFrameworkCore;

namespace ezZkvi.Services
{
    public static class UserDeletionService
    {
        public static async Task ObrisiKorisnikaSaSadrzajemAsync(ApplicationDbContext context, string korisnikId)
        {
            if (string.IsNullOrWhiteSpace(korisnikId))
            {
                return;
            }

            var sesijaIds = await context.KvizSesije
                .Where(s => s.StudentId == korisnikId)
                .Select(s => s.ID)
                .ToListAsync();

            if (sesijaIds.Count > 0)
            {
                var stavkeSesija = await context.KvizSesijaPitanja
                    .Where(ksp => sesijaIds.Contains(ksp.KvizSesijaId))
                    .ToListAsync();

                context.KvizSesijaPitanja.RemoveRange(stavkeSesija);

                var sesije = await context.KvizSesije
                    .Where(s => sesijaIds.Contains(s.ID))
                    .ToListAsync();

                context.KvizSesije.RemoveRange(sesije);
            }

            var feedback = await context.Feedback
                .Where(f => f.KorisnikId == korisnikId)
                .ToListAsync();

            context.Feedback.RemoveRange(feedback);

            var statistike = await context.StudentStatistike
                .Where(s => s.KorisnikId == korisnikId)
                .ToListAsync();

            context.StudentStatistike.RemoveRange(statistike);
        }
    }
}
