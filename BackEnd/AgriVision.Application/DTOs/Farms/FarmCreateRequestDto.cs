using System.Collections.Generic;

namespace AgriVision.Application.DTOs.Farms
{
    public class FarmCreateRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? SoilType { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public string LengthUnit { get; set; } = "m";
        public double? Temperature { get; set; }
        public double? Humidity { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public List<FarmFieldCreateDto> Fields { get; set; } = new();
    }
}
