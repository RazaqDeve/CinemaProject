using BookingService.Entities.Enum;

namespace BookingService.DTOs.ShowtimeSeat;

public class ShowtimeSeatDto
{
    public Guid SeatId { get; set; }

    public SeatStatus Status { get; set; }
}