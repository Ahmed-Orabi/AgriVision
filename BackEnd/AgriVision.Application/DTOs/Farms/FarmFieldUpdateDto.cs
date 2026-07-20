using System;

namespace AgriVision.Application.DTOs.Farms
{
    public class FarmFieldUpdateDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Crop { get; set; } = string.Empty;
        public double XStart { get; set; }
        public double XEnd { get; set; }
        public double YStart { get; set; }
        public double YEnd { get; set; }
        public string? Status { get; set; }
        public double? Temperature { get; set; }
        public double? Moisture { get; set; }
        public double? SoilTemperature { get; set; }
        public double? SoilMoisture { get; set; }
    }
}
