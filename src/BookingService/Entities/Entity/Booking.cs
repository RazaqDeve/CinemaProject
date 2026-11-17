using BookingService.Entities.Enum;

namespace BookingService.Entities.Entity
{
    public class Booking
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid ShowtimeId { get; set; }

        public decimal TotalPrice { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    }
}