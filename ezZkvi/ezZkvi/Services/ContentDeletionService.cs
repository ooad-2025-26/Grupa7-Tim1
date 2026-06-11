using ezZkvi.Data;
using ezZkvi.Models;
using Microsoft.EntityFrameworkCore;

namespace ezZkvi.Services
{
    public static class ContentDeletionService
    {
        public static async Task ObrisiPredmetSaSadrzajemAsync(ApplicationDbContext context, int predmetId)
        {
            var oblastIds = await context.Oblast
                .Where(o => o.PredmetId == predmetId)
                .Select(o => o.Id)
                .ToListAsync();

            var pitanjeIds = await context.Pitanje
                .Where(p => p.PredmetId == predmetId)
                .Select(p => p.Id)
                .ToListAsync();

            var sesijaIds = await NadjiSesijeZaPredmetAsync(context, predmetId, oblastIds, pitanjeIds);
            await ObrisiSesijeAsync(context, sesijaIds);
            await ObrisiPitanjaBezBrisanjaSesijaAsync(context, pitanjeIds);

            var feedback = await context.Feedback
                .Where(f => f.PredmetId == predmetId)
                .ToListAsync();

            context.Feedback.RemoveRange(feedback);

            var statistike = await context.StudentStatistike
                .Where(s => s.PredmetId == predmetId)
                .ToListAsync();

            context.StudentStatistike.RemoveRange(statistike);

            var oblasti = await context.Oblast
                .Where(o => o.PredmetId == predmetId)
                .ToListAsync();

            context.Oblast.RemoveRange(oblasti);
        }

        public static async Task ObrisiOblastSaSadrzajemAsync(ApplicationDbContext context, int oblastId)
        {
            var pitanjeIds = await context.Pitanje
                .Where(p => p.OblastId == oblastId)
                .Select(p => p.Id)
                .ToListAsync();

            var sesijaIds = await NadjiSesijeZaOblastAsync(context, oblastId, pitanjeIds);
            await ObrisiSesijeAsync(context, sesijaIds);
            await ObrisiPitanjaBezBrisanjaSesijaAsync(context, pitanjeIds);
        }

        public static async Task ObrisiPitanjeSaSadrzajemAsync(ApplicationDbContext context, int pitanjeId)
        {
            await ObrisiPitanjaBezBrisanjaSesijaAsync(context, new List<int> { pitanjeId });
        }

        private static async Task<List<int>> NadjiSesijeZaPredmetAsync(
            ApplicationDbContext context,
            int predmetId,
            List<int> oblastIds,
            List<int> pitanjeIds)
        {
            var sesijaIds = new HashSet<int>();

            var direktneSesije = await context.KvizSesije
                .Where(s => s.PredmetId == predmetId || (s.OblastId.HasValue && oblastIds.Contains(s.OblastId.Value)))
                .Select(s => s.ID)
                .ToListAsync();

            foreach (var id in direktneSesije)
            {
                sesijaIds.Add(id);
            }

            if (pitanjeIds.Count > 0)
            {
                var sesijeKrozPitanja = await context.KvizSesijaPitanja
                    .Where(ksp => pitanjeIds.Contains(ksp.PitanjeId))
                    .Select(ksp => ksp.KvizSesijaId)
                    .Distinct()
                    .ToListAsync();

                foreach (var id in sesijeKrozPitanja)
                {
                    sesijaIds.Add(id);
                }
            }

            return sesijaIds.ToList();
        }

        private static async Task<List<int>> NadjiSesijeZaOblastAsync(
            ApplicationDbContext context,
            int oblastId,
            List<int> pitanjeIds)
        {
            var sesijaIds = new HashSet<int>();

            var direktneSesije = await context.KvizSesije
                .Where(s => s.OblastId == oblastId)
                .Select(s => s.ID)
                .ToListAsync();

            foreach (var id in direktneSesije)
            {
                sesijaIds.Add(id);
            }

            if (pitanjeIds.Count > 0)
            {
                var sesijeKrozPitanja = await context.KvizSesijaPitanja
                    .Where(ksp => pitanjeIds.Contains(ksp.PitanjeId))
                    .Select(ksp => ksp.KvizSesijaId)
                    .Distinct()
                    .ToListAsync();

                foreach (var id in sesijeKrozPitanja)
                {
                    sesijaIds.Add(id);
                }
            }

            return sesijaIds.ToList();
        }

        private static async Task ObrisiSesijeAsync(ApplicationDbContext context, List<int> sesijaIds)
        {
            if (sesijaIds.Count == 0)
            {
                return;
            }

            var stavkeSesija = await context.KvizSesijaPitanja
                .Where(ksp => sesijaIds.Contains(ksp.KvizSesijaId))
                .ToListAsync();

            context.KvizSesijaPitanja.RemoveRange(stavkeSesija);

            var sesije = await context.KvizSesije
                .Where(s => sesijaIds.Contains(s.ID))
                .ToListAsync();

            context.KvizSesije.RemoveRange(sesije);
        }

        private static async Task ObrisiPitanjaBezBrisanjaSesijaAsync(ApplicationDbContext context, List<int> pitanjeIds)
        {
            if (pitanjeIds.Count == 0)
            {
                return;
            }

            var preostaleStavkeSesija = await context.KvizSesijaPitanja
                .Where(ksp => pitanjeIds.Contains(ksp.PitanjeId))
                .ToListAsync();

            context.KvizSesijaPitanja.RemoveRange(preostaleStavkeSesija);

            var odgovori = await context.Odgovor
                .Where(o => pitanjeIds.Contains(o.PitanjeId))
                .ToListAsync();

            context.Odgovor.RemoveRange(odgovori);

            var pitanja = await context.Pitanje
                .Where(p => pitanjeIds.Contains(p.Id))
                .ToListAsync();

            context.Pitanje.RemoveRange(pitanja);
        }
    }
}
