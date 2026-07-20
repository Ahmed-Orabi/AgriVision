using System.Threading.Tasks;
using AgriVision.Application.DTOs.Geocoding;

namespace AgriVision.Application.Interfaces
{
    public interface IGeocodingService
    {
        Task<ReverseGeocodeResultDto?> ReverseAsync(double latitude, double longitude);
    }
}
