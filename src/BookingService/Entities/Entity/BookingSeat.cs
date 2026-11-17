

namespace BookingService.Entities.Entity
{
    public class BookingSeat
    {
        public Guid Id { get; set; }

        public Guid BookingId { get; set; }

        public Guid SeatId { get; set; }

        public decimal Price { get; set; }

        public Booking Booking { get; set; } = null!;
    }
}