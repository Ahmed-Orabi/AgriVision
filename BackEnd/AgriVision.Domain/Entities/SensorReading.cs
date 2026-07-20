using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriVision.Domain.Entities
{
    public class SensorReading
    {
        public Guid Id { get; set; } // Sensor reading ID

        public Guid SensorId { get; set; } // Sensor ID (FK)

        public Sensor Sensor { get; set; } = default!; // Sensor associated with this reading

        public DateTime Timestamp { get; set; } // Measurement timestamp

        public double Value { get; set; } // Measurement value

        public string Unit { get; set; } = "N/A"; // Measurement unit

        public bool IsAlert { get; set; } // Indicates whether this reading is an alert

        public string? AlertType { get; set; } // Type of alert (if any)
    }
}