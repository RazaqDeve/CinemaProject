

using CatalogService.Entities.Enums;

namespace CatalogService.Entities
{
    public class Movie
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int DurationMinutes { get; set; }

        public string Director {get; set;} = string.Empty;

        public Genre Genre {get; set;}

        public DateTime ReleaseDate { get; set; }

        public ICollection<Showtime> Showtimes {get; set;} = new List<Showtime>();
    }
}