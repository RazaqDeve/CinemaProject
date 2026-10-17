using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogService.Entities
{
    public class Showtime
    {
        public Guid Id { get; set; }

        public Guid MovieId { get; set; }

        public Guid HallId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal Price { get; set; }

        public Movie Movie { get; set; } = null!;
        
        public Hall Hall { get; set; } = null!;
    }
}