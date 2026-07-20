using System.Threading.Tasks;
using AgriVision.Application.DTOs.Weather;

namespace AgriVision.Application.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherSnapshotDto> GetCurrentAsync(double latitude, double longitude);
    }
}
