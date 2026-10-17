
namespace CatalogService.DTOs
{
    public class SeatDto
    {
        public Guid? Id { get; set; }

        public Guid RowId { get; set; }

        public int SeatNumber { get; set; }
    }
}