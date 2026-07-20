using AgriVision.Application.DTOs.Farms;
using AgriVision.Application.Interfaces;
using AgriVision.Application.Subscriptions;
using AgriVision.Domain.Entities;
using AgriVision.Domain.ValueObjects;
using AgriVision.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgriVision.Infrastructure.Services
{
    public class FarmService : IFarmService
    {
        private readonly AgriVisionDbContext _context;

        public FarmService(AgriVisionDbContext context)
        {
            _context = context;
        }

        public async Task<List<FarmSummaryDto>> GetUserFarmsAsync(string userId)
        {
            return await _context.Farms
                .AsNoTracking()
                .Where(f => f.OwnerId == userId)
                .Select(f => new FarmSummaryDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Address = f.Address,
                    SoilType = f.SoilType,
                    Length = f.Length,
                    Width = f.Width,
                    LengthUnit = f.LengthUnit,
                    FieldsCount = f.Fields.Count,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<FarmDetailsDto?> GetByIdAsync(string userId, Guid farmId)
        {
            var farm = await _context.Farms
                .AsNoTracking()
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.Id == farmId && f.OwnerId == userId);

            if (farm == null) return null;

            return new FarmDetailsDto
            {
                Id = farm.Id,
                Name = farm.Name,
                Address = farm.Address,
                Description = farm.Description,
                SoilType = farm.SoilType,
                Length = farm.Length,
                Width = farm.Width,
                LengthUnit = farm.LengthUnit,
                Temperature = farm.Temperature,
                Humidity = farm.Humidity,
                Latitude = farm.Location?.Latitude,
                Longitude = farm.Location?.Longitude,
                Fields = farm.Fields
                    .OrderBy(x => x.Name)
                    .Select(ToFieldDto)
                    .ToList()
            };
        }

        public async Task<FarmStateDto?> GetStateAsync(string userId, Guid farmId)
        {
            var farm = await _context.Farms
                .AsNoTracking()
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.Id == farmId && f.OwnerId == userId);

            if (farm == null) return null;

            var fields = farm.Fields.OrderBy(x => x.Name).ToList();
            var totalArea = farm.Length * farm.Width;
            var usedArea = fields.Sum(GetFieldArea);
            var coverage = totalArea > 0 ? (int)Math.Round((usedArea / totalArea) * 100) : 0;
            var alerts = fields.Count(IsAlertStatus);

            var tempValues = fields.Where(x => x.Temperature.HasValue).Select(x => x.Temperature!.Value).ToList();
            var moistureValues = fields.Where(x => x.Moisture.HasValue).Select(x => x.Moisture!.Value).ToList();
            var humidityValues = fields.Where(x => x.SoilMoisture.HasValue).Select(x => x.SoilMoisture!.Value).ToList();

            return new FarmStateDto
            {
                Id = farm.Id,
                Name = farm.Name,
                Address = farm.Address,
                Description = farm.Description,
                SoilType = farm.SoilType,
                Length = farm.Length,
                Width = farm.Width,
                LengthUnit = farm.LengthUnit,
                Latitude = farm.Location?.Latitude,
                Longitude = farm.Location?.Longitude,
                Temperature = farm.Temperature ?? (tempValues.Any() ? tempValues.Average() : null),
                Moisture = moistureValues.Any() ? moistureValues.Average() : null,
                Humidity = farm.Humidity ?? (humidityValues.Any() ? humidityValues.Average() : null),
                TotalFields = fields.Count,
                Alerts = alerts,
                CoveragePercent = coverage,
                Fields = fields.Select(ToFieldDto).ToList()
            };
        }

        public async Task<Guid> CreateAsync(string userId, FarmCreateRequestDto request)
        {
            ValidateFarmRequest(request.Length, request.Width, request.Fields);

            if (!request.Latitude.HasValue || !request.Longitude.HasValue)
                throw new Exception("Latitude and Longitude are required.");

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception("User not found.");

            var limits = SubscriptionPlanRules.GetLimits(user.SubscriptionPlan);
            if (limits.FarmLimit > 0)
            {
                var existingFarms = await _context.Farms.CountAsync(f => f.OwnerId == userId);
                if (existingFarms >= limits.FarmLimit)
                    throw new Exception("Farm limit reached for your subscription plan.");
            }

            var location = new Location(request.Latitude.Value, request.Longitude.Value);

            var farm = new Farm
            {
                Name = request.Name,
                Address = request.Address,
                Description = request.Description,
                SoilType = request.SoilType,
                Length = request.Length,
                Width = request.Width,
                LengthUnit = request.LengthUnit,
                Temperature = request.Temperature,
                Humidity = request.Humidity,
                Location = location,
                OwnerId = userId
            };

            foreach (var field in request.Fields)
            {
                farm.Fields.Add(new FarmField
                {
                    Name = field.Name,
                    Crop = field.Crop,
                    XStart = field.XStart,
                    XEnd = field.XEnd,
                    YStart = field.YStart,
                    YEnd = field.YEnd,
                    Status = field.Status ?? "Healthy",
                    SoilTemperature = field.SoilTemperature,
                    SoilMoisture = field.SoilMoisture,
                    UpdatedAtUtc = DateTime.UtcNow
                });
            }

            _context.Farms.Add(farm);
            await _context.SaveChangesAsync();
            return farm.Id;
        }

        public async Task<bool> UpdateAsync(string userId, Guid farmId, FarmUpdateRequestDto request)
        {
            ValidateFarmRequest(request.Length, request.Width, request.Fields);

            var farm = await _context.Farms
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(f => f.Id == farmId && f.OwnerId == userId);

            if (farm == null) return false;

            farm.Name = request.Name;
            farm.Address = request.Address;
            farm.Description = request.Description;
            farm.SoilType = request.SoilType;
            farm.Length = request.Length;
            farm.Width = request.Width;
            farm.LengthUnit = request.LengthUnit;
            farm.Temperature = request.Temperature;
            farm.Humidity = request.Humidity;

            var latitude = request.Latitude ?? farm.Location.Latitude;
            var longitude = request.Longitude ?? farm.Location.Longitude;
            farm.Location = new Location(latitude, longitude);

            var existing = farm.Fields.ToDictionary(f => f.Id, f => f);
            var keep = new HashSet<Guid>();

            foreach (var fieldDto in request.Fields)
            {
                if (fieldDto.Id.HasValue && existing.TryGetValue(fieldDto.Id.Value, out var field))
                {
                    field.Name = fieldDto.Name;
                    field.Crop = fieldDto.Crop;
                    field.XStart = fieldDto.XStart;
                    field.XEnd = fieldDto.XEnd;
                    field.YStart = fieldDto.YStart;
                    field.YEnd = fieldDto.YEnd;
                    field.Status = fieldDto.Status ?? field.Status;
                    if (fieldDto.Temperature.HasValue) field.Temperature = fieldDto.Temperature;
                    if (fieldDto.Moisture.HasValue) field.Moisture = fieldDto.Moisture;
                    if (fieldDto.SoilTemperature.HasValue) field.SoilTemperature = fieldDto.SoilTemperature;
                    if (fieldDto.SoilMoisture.HasValue) field.SoilMoisture = fieldDto.SoilMoisture;
                    field.UpdatedAtUtc = DateTime.UtcNow;
                    keep.Add(field.Id);
                    continue;
                }

                var newField = new FarmField
                {
                    Name = fieldDto.Name,
                    Crop = fieldDto.Crop,
                    XStart = fieldDto.XStart,
                    XEnd = fieldDto.XEnd,
                    YStart = fieldDto.YStart,
                    YEnd = fieldDto.YEnd,
                    Status = fieldDto.Status ?? "Healthy",
                    Temperature = fieldDto.Temperature,
                    Moisture = fieldDto.Moisture,
                    SoilTemperature = fieldDto.SoilTemperature,
                    SoilMoisture = fieldDto.SoilMoisture,
                    UpdatedAtUtc = DateTime.UtcNow,
                    FarmId = farm.Id
                };

                _context.FarmFields.Add(newField);
                keep.Add(newField.Id);
            }

            var toRemove = farm.Fields.Where(f => !keep.Contains(f.Id)).ToList();
            if (toRemove.Any())
            {
                _context.FarmFields.RemoveRange(toRemove);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string userId, Guid farmId)
        {
            var farm = await _context.Farms
                .FirstOrDefaultAsync(f => f.Id == farmId && f.OwnerId == userId);

            if (farm == null) return false;

            _context.Farms.Remove(farm);
            await _context.SaveChangesAsync();
            return true;
        }

        private static FarmFieldDto ToFieldDto(FarmField field)
        {
            return new FarmFieldDto
            {
                Id = field.Id,
                Name = field.Name,
                Crop = field.Crop,
                XStart = field.XStart,
                XEnd = field.XEnd,
                YStart = field.YStart,
                YEnd = field.YEnd,
                Status = field.Status,
                Temperature = field.Temperature,
                Moisture = field.Moisture,
                SoilTemperature = field.SoilTemperature,
                SoilMoisture = field.SoilMoisture,
                UpdatedAtUtc = field.UpdatedAtUtc
            };
        }

        private static void ValidateFarmRequest(double length, double width, IEnumerable<FarmFieldCreateDto> fields)
        {
            if (length <= 0 || width <= 0)
                throw new Exception("Farm dimensions must be greater than zero.");

            var list = fields.Select(f => new Rect
            {
                Name = f.Name,
                XStart = f.XStart,
                XEnd = f.XEnd,
                YStart = f.YStart,
                YEnd = f.YEnd
            }).ToList();

            ValidateRectangles(length, width, list);
        }

        private static void ValidateFarmRequest(double length, double width, IEnumerable<FarmFieldUpdateDto> fields)
        {
            if (length <= 0 || width <= 0)
                throw new Exception("Farm dimensions must be greater than zero.");

            var list = fields.Select(f => new Rect
            {
                Name = f.Name,
                XStart = f.XStart,
                XEnd = f.XEnd,
                YStart = f.YStart,
                YEnd = f.YEnd
            }).ToList();

            ValidateRectangles(length, width, list);
        }

        private static void ValidateRectangles(double length, double width, List<Rect> list)
        {
            foreach (var rect in list)
            {
                if (rect.XStart > rect.XEnd || rect.YStart > rect.YEnd)
                    throw new Exception($"Invalid field range for {rect.Name}.");

                if (rect.XStart < 0 || rect.YStart < 0 || rect.XEnd > length || rect.YEnd > width)
                    throw new Exception($"Field {rect.Name} is outside farm bounds.");
            }

            for (var i = 0; i < list.Count; i++)
            {
                for (var j = i + 1; j < list.Count; j++)
                {
                    if (IsOverlapping(list[i], list[j]))
                        throw new Exception($"Field {list[i].Name} overlaps with {list[j].Name}.");
                }
            }
        }

        private static bool IsOverlapping(Rect a, Rect b)
        {
            var overlapX = a.XStart < b.XEnd && a.XEnd > b.XStart;
            var overlapY = a.YStart < b.YEnd && a.YEnd > b.YStart;
            return overlapX && overlapY;
        }

        private static double GetFieldArea(FarmField field)
        {
            var length = Math.Max(0, field.XEnd - field.XStart);
            var width = Math.Max(0, field.YEnd - field.YStart);
            return length * width;
        }

        private static bool IsAlertStatus(FarmField field)
        {
            var status = field.Status ?? string.Empty;
            return status.Contains("warn", StringComparison.OrdinalIgnoreCase)
                || status.Contains("critical", StringComparison.OrdinalIgnoreCase);
        }

        private sealed class Rect
        {
            public string Name { get; set; } = string.Empty;
            public double XStart { get; set; }
            public double XEnd { get; set; }
            public double YStart { get; set; }
            public double YEnd { get; set; }
        }
    }
}
