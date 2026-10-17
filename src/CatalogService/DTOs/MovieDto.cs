using CatalogService.Entities.Enums;


namespace CatalogService.DTOs
{
    public class MovieDto
    {
        public Guid? Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int DurationMinutes { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string Director {get; set;} = string.Empty;

         public Genre Genre {get; set;}

    }
}