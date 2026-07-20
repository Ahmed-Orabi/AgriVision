using System;

namespace AgriVision.Application.DTOs.Farms
{
    public class FarmSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? SoilType { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public string LengthUnit { get; set; } = "m";
        public int FieldsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
