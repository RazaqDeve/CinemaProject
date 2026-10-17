using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogService.Entities
{
    public class Seat
    {
        public Guid Id { get; set; }

        public Guid RowId { get; set; }

        public int SeatNumber { get; set; }

        public Row Row {get; set;} = null!;
    }
}