

namespace BookingService.Entities.Entity
{
    public class ShowtimeSeat
    {
        public Guid Id { get; set; }

        public Guid ShowtimeId { get; set; }

        public Guid SeatId { get; set; }

        public SeatStatus Status { get; set; } = SeatStatus.Available;

        public Guid? BookingId { get; set; }

        public DateTime? LockedUntil { get; set; }

        public Booking? Booking { get; set; }
    }
}