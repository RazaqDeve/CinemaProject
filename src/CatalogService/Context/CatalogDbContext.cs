using Microsoft.EntityFrameworkCore;
using CatalogService.Entities;

namespace CatalogService.Context
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Row> Rows { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }

        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            model.Entity<Hall>()
            .HasOne(h => h.Cinema)
            .WithMany(c => c.Halls)
            .HasForeignKey(h => h.CinemaId)
            .OnDelete(DeleteBehavior.NoAction);

            model.Entity<Row>()
            .HasOne(r => r.Hall)
            .WithMany(h => h.Rows)
            .HasForeignKey(r => r.HallId)
            .OnDelete(DeleteBehavior.NoAction);

            model.Entity<Seat>()
            .HasOne(s => s.Row)
            .WithMany(r => r.Seats)
            .HasForeignKey(s => s.RowId)
            .OnDelete(DeleteBehavior.NoAction);

            model.Entity<Showtime>()
            .HasOne(sh => sh.Movie)
            .WithMany(m => m.Showtimes)
            .HasForeignKey(sh => sh.MovieId)
            .OnDelete(DeleteBehavior.NoAction);

            model.Entity<Showtime>()
            .HasOne(sh => sh.Hall)
            .WithMany(h => h.Showtimes)
            .HasForeignKey(sh => sh.HallId)
            .OnDelete(DeleteBehavior.NoAction);
        }
    }
}