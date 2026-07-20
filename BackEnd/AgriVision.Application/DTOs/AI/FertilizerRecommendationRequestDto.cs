namespace AgriVision.Application.DTOs.AI;

public class FertilizerRecommendationRequestDto
{
    public double Temperature { get; set; }
    public double Moisture { get; set; }
    public double Rainfall { get; set; }
    public double PH { get; set; }

    public double Nitrogen { get; set; }
    public double Phosphorous { get; set; }
    public double Potassium { get; set; }

    public double Carbon { get; set; }

    public string Soil { get; set; } = string.Empty;
    public string Crop { get; set; } = string.Empty;
}
