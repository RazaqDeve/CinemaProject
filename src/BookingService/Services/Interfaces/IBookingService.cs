using BookingService.DTOs.Booking;
using BookingService.DTOs.ShowtimeSeat;


namespace BookingService.Services.Interfaces
{
    public interface IBookingService
    {
        // ایجاد رزرو و انتخاب موقت صندلی‌ها
        Task<BookingDto> CreateAsync(Guid userId, CreateBookingDto dto, CancellationToken cancellationToken = default);

        // دریافت اطلاعات یک رزرو
        Task<BookingDto?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);

        // دریافت رزروهای یک کاربر
        Task<List<BookingDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        // دریافت وضعیت صندلی‌های یک سانس
        Task<List<ShowtimeSeatDto>> GetSeatsByShowtimeAsync(Guid showtimeId, CancellationToken cancellationToken = default);

        // لغو رزرو
        Task<bool> CancelAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken = default);
    }
}
