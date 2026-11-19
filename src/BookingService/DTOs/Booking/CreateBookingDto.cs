


namespace BookingService.DTOs.Booking
{
    public class CreateBookingDto
    {
        public Guid ShowtimeId { get; set; }

        public List<Guid> SeatIds { get; set; } = new();
    }
}