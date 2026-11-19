using BookingService.Entities.Enum;

namespace BookingService.DTOs.Booking;

public class BookingDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ShowtimeId { get; set; }

    public decimal TotalPrice { get; set; }

    public BookingStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<BookingSeatDto> BookingSeats { get; set; } = new();
}