using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgriVision.Domain.Enums;

namespace AgriVision.Domain.Entities
{
    public class Sensor
    {
        public Guid Id { get; set; } // Sensor KeyId

        public string Name { get; set; } = default!; // Sensor Name 

        public SensorType Type { get; set; } // Sensor Type

        public bool IsActive { get; set; } = true; // Is Sensor Active?

        public DateTime InstalledAt { get; set; } = DateTime.UtcNow; // Sensor Date

        public Guid FarmId { get; set; } // Id of Farm of the Sensor (FK)

        public Farm Farm { get; set; } = default!; // Farm of the Sensor

        public ICollection<SensorReading> Readings { get; set; } = new List<SensorReading>(); // Sensor Readings
    }
}
