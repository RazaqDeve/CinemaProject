using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogService.DTOs
{
    public class HallDto
    {
        public Guid? Id { get; set; }

        public Guid CinemaId { get; set; }

        public string Name { get; set; } = string.Empty;

    }
}