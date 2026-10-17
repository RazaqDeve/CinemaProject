
namespace CatalogService.Entities
{
    public class Hall
    {
        public Guid Id { get; set; }

        public Guid CinemaId { get; set; }

        public string Name { get; set; } = string.Empty;

        public Cinema Cinema {get; set;} = null!;

        public ICollection<Row> Rows {get; set;} = new List<Row>();

        public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}