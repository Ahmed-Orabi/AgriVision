using System;
using System.Collections.Generic;

namespace AgriVision.Application.DTOs.Farms
{
    public class FarmStateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? SoilType { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public string LengthUnit { get; set; } = "m";
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Temperature { get; set; }
        public double? Moisture { get; set; }
        public double? Humidity { get; set; }
        public int TotalFields { get; set; }
        public int Alerts { get; set; }
        public int CoveragePercent { get; set; }
        public List<FarmFieldDto> Fields { get; set; } = new();
    }
}
