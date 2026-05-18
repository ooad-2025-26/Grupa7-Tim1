using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ezZkvi.Models;

namespace ezZkvi.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Predmet> Predmet { get; set; }
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
            modelBuilder.Entity<Pitanje>().ToTable("Pitanje");
            modelBuilder.Entity<Odgovor>().ToTable("Odgovor");
            modelBuilder.Entity<KvizSesija>().ToTable("KvizSesija");
            modelBuilder.Entity<Feedback>().ToTable("Feedback");
            modelBuilder.Entity<KvizSesijaPitanje>().ToTable("KvizSesijaPitanje");

            base.OnModelCreating(modelBuilder);
        }
    }
}
