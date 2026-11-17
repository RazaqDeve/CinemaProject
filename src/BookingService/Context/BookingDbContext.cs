
using BookingService.Entities.Entity;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Context
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }


        public DbSet<Booking> Bookings { get; set; }

        public DbSet<BookingSeat> BookingSeats { get; set; }

        public DbSet<ShowtimeSeat> ShowtimeSeats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Booking -> BookingSeat
            modelBuilder.Entity<Booking>()
              .HasMany(b => b.BookingSeats)
              .WithOne(bs => bs.Booking)
              .HasForeignKey(bs => bs.BookingId)
              .OnDelete(DeleteBehavior.Cascade);

            // Booking -> ShowtimeSeat
            modelBuilder.Entity<Booking>()
              .HasMany<ShowtimeSeat>()
              .WithOne(ss => ss.Booking)
              .HasForeignKey(ss => ss.BookingId)
              .IsRequired(false)
              .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.Property(b => b.TotalPrice)
                    .HasPrecision(18, 2);

                entity.Property(b => b.Status)
                    .IsRequired();

                entity.Property(b => b.CreatedAt)
                    .IsRequired();

                entity.HasIndex(b => b.UserId);

                entity.HasIndex(b => b.ShowtimeId);
            });

            modelBuilder.Entity<BookingSeat>(entity =>
            {
                entity.Property(bs => bs.Price)
                    .HasPrecision(18, 2);

                entity.HasIndex(bs => bs.SeatId);
            });

            modelBuilder.Entity<ShowtimeSeat>(entity =>
            {
                entity.Property(ss => ss.Status)
                    .IsRequired();
                // Unique Index
                entity.HasIndex(ss => new
                {
                    ss.ShowtimeId,
                    ss.SeatId
                })
                .IsUnique()
                .HasDatabaseName("IX_ShowtimeSeats_ShowtimeId_SeatId");

                entity.HasIndex(ss => ss.BookingId);
            });
        }
    }
}