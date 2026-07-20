using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgriVision.Application.DTOs.Farms;

namespace AgriVision.Application.Interfaces
{
    public interface IFarmService
    {
        Task<List<FarmSummaryDto>> GetUserFarmsAsync(string userId);
        Task<FarmDetailsDto?> GetByIdAsync(string userId, Guid farmId);
        Task<FarmStateDto?> GetStateAsync(string userId, Guid farmId);
        Task<Guid> CreateAsync(string userId, FarmCreateRequestDto request);
        Task<bool> UpdateAsync(string userId, Guid farmId, FarmUpdateRequestDto request);
        Task<bool> DeleteAsync(string userId, Guid farmId);
    }
}
