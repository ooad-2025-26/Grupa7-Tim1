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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Predmet>().ToTable("Predmet");
            modelBuilder.Entity<Pitanje>().ToTable("Pitanje");
            modelBuilder.Entity<Odgovor>().ToTable("Odgovor");
            base.OnModelCreating(modelBuilder);
        }

    }
}
