using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ezZkvi.Models;

namespace ezZkvi.Data
{
    public class ApplicationDbContext : IdentityDbContext<Korisnik>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Predmet> Predmet { get; set; }
        public DbSet<Oblast> Oblast { get; set; }
        public DbSet<Pitanje> Pitanje { get; set; }
        public DbSet<Odgovor> Odgovor { get; set; }
        public DbSet<KvizSesija> KvizSesije { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<KvizSesijaPitanje> KvizSesijaPitanja { get; set; }

        public DbSet<ezZkvi.Models.Administrator> Administrator { get; set; } = default!;
        public DbSet<ezZkvi.Models.Moderator> Moderator { get; set; } = default!;
        public DbSet<ezZkvi.Models.Student> Student { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Predmet>().ToTable("Predmet");
            modelBuilder.Entity<Oblast>().ToTable("Oblast");
            modelBuilder.Entity<Pitanje>().ToTable("Pitanje");
            modelBuilder.Entity<Odgovor>().ToTable("Odgovor");
            modelBuilder.Entity<KvizSesija>().ToTable("KvizSesija");
            modelBuilder.Entity<Feedback>().ToTable("Feedback");
            modelBuilder.Entity<KvizSesijaPitanje>().ToTable("KvizSesijaPitanje");

            modelBuilder.Entity<Oblast>()
                .HasOne(o => o.Predmet)
                .WithMany(p => p.Oblasti)
                .HasForeignKey(o => o.PredmetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pitanje>()
                .HasOne(p => p.Predmet)
                .WithMany(p => p.Pitanja)
                .HasForeignKey(p => p.PredmetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pitanje>()
                .HasOne(p => p.Oblast)
                .WithMany(o => o.Pitanja)
                .HasForeignKey(p => p.OblastId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KvizSesija>()
                .HasOne(s => s.Oblast)
                .WithMany()
                .HasForeignKey(s => s.OblastId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}
