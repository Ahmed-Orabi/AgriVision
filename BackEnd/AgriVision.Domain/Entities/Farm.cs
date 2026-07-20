using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgriVision.Domain.ValueObjects;

namespace AgriVision.Domain.Entities
{
    public class Farm
    {
        public Guid Id { get; set; } // Farm KeyId

        public string Name { get; set; } = default!; // Farm Name

        public string? Description { get; set; } // Farm Description

        public string? Address { get; set; } // Farm Address

        public string? SoilType { get; set; } // Farm Soil Type

        public double Length { get; set; } // Farm Length

        public double Width { get; set; } // Farm Width

        public string LengthUnit { get; set; } = "m"; // Unit of Length/Width

        public double? Temperature { get; set; } // Farm Temperature

        public double? Humidity { get; set; } // Farm Humidity

        public Location Location { get; set; } = default!; // Farm Location

        public string OwnerId { get; set; } = default!; // Farm Owner Id (FK)

        public ApplicationUser Owner { get; set; } = default!; // Farm Owner

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Farm Creation Date

        public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>(); // Sensors In Farm

        public ICollection<FarmField> Fields { get; set; } = new List<FarmField>(); // Zones/Fields In Farm
    }
}
