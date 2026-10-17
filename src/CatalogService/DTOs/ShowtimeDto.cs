

namespace CatalogService.DTOs
{
    public class ShowtimeDto
    {
          public Guid? Id { get; set; }

        public Guid MovieId { get; set; }

        public Guid HallId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal Price { get; set; }

    }
}