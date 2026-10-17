using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogService.Entities
{
    public class Row
    {
        public Guid Id { get; set; }

        public Guid HallId { get; set; }

        public int RowNumber { get; set; }

        public Hall Hall { get; set; } = null!;

        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}